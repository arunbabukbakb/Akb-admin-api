using AutoMapper;
using System.Text.Json;
using Models;
using Data;
using Data.Repository.IRepository;
using Models;
using Models.DtoModels;
using Models.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models.Enums;

namespace ProductCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ApprovalController(IUnitOfWork db, IMapper mapper, IUserService userService)
        {
            _db = db;
            _mapper = mapper;
            _userService = userService;
        }

        // GET: api/Approval
        [HttpGet]
        public async Task<IActionResult> Get(string? filter,
            string? search,
            string? sortBy = "CreatedDate",
            string? sortDirection = "desc",
            int pageNumber = 1,
            int pageSize = 10)
        {
            Response result = new Response(true, "");
            try
            {
                Dictionary<string, FilterCondition>? filters = null;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filters = JsonSerializer.Deserialize<Dictionary<string, FilterCondition>>(filter, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                IQueryable<Approval> query = _db.Approval.GetAllQueriable(null,
                    "ApprovedByUser,CreatedByUser,ModifiedByUser", dynamicFilters: filters);

                if (!string.IsNullOrEmpty(search))
                {
                    query = QueryHelper.ApplySearch(
                        query,
                        search,
                        x => x.RefNo!,
                        x => x.Remarks!
                    );
                }

                int totalRecords = query.Count();

                query = QueryHelper.ApplySorting(query, sortBy!, sortDirection!);

                var pagedData = QueryHelper.ApplyPagination(query, pageNumber, pageSize).ToList();

                var dtoData = _mapper.Map<List<ApprovalDto>>(pagedData);

                result.data = dtoData;
                result.pagination = new Pagination
                {
                    PageNumber = pageNumber < 1 ? 1 : pageNumber,
                    PageSize = pageSize < 1 ? 10 : pageSize,
                    TotalRecords = totalRecords
                };
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error retrieving approvals.";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        // GET: api/Approval/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Response result = new Response(true, "");
            try
            {
                var obj = _db.Approval.GetFirstOrDefault(
                    x => x.Id == id,
                    "ApprovedByUser,CreatedByUser,ModifiedByUser"
                );

                if (obj == null)
                {
                    result.status = false;
                    result.message = "Approval record not found.";
                }
                else
                {
                    result.data = _mapper.Map<ApprovalDto>(obj);
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error retrieving approval details.";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        // POST: api/Approval
        [HttpPost]
        public IActionResult Post(ApprovalDto dto)
        {
            Response result = new Response(true);
            try
            {
                if (!ModelState.IsValid)
                {
                    result.status = false;
                    result.message = "Model state is invalid.";
                    return Ok(result);
                }

                int currentUserId = _userService.GetUserId();
                Approval obj = _mapper.Map<Approval>(dto);

                _db.BeginTransaction();

                if (obj.Id > 0)
                {
                    // Update approval log
                    var existingApproval = _db.Approval.GetFirstOrDefault(x => x.Id == obj.Id);
                    if (existingApproval == null)
                    {
                        result.status = false;
                        result.message = "Approval record not found.";
                        _db.Rollback();
                        return Ok(result);
                    }

                    existingApproval.ApprovedStatus = obj.ApprovedStatus;
                    existingApproval.Remarks = obj.Remarks;
                    existingApproval.ApprovedBy = obj.ApprovedStatus ? currentUserId : null;
                    existingApproval.ModifiedBy = currentUserId;
                    existingApproval.ModifiedDate = DateTime.Now;

                    _db.Approval.Update(existingApproval);
                    _db.Save();

                    // If approved, trigger business side effects
                    if (existingApproval.ApprovedStatus)
                    {
                        ProcessModuleApproval(existingApproval.RefType, existingApproval.RefId, currentUserId);
                    }
                    else
                    {
                        ProcessModuleReject(obj.RefType, obj.RefId, currentUserId);
                    }

                    _db.Commit();
                    result.message = "Approval log updated successfully.";
                }
                else
                {
                    // Create new approval log
                    obj.ApprovedBy = obj.ApprovedStatus ? currentUserId : null;
                    obj.CreatedBy = currentUserId;
                    obj.CreatedDate = DateTime.Now;
                    obj.ModifiedBy = currentUserId;
                    obj.ModifiedDate = DateTime.Now;

                    _db.Approval.Add(obj);
                    _db.Save();

                    // If approved, trigger business side effects
                    if (obj.ApprovedStatus)
                    {
                        ProcessModuleApproval(obj.RefType, obj.RefId, currentUserId);
                    }
                    else
                    {
                        ProcessModuleReject(obj.RefType, obj.RefId, currentUserId);
                    }

                    _db.Commit();
                    result.message = "Approval log saved successfully.";
                }
            }
            catch (Exception ex)
            {
                _db.Rollback();
                result.status = false;
                result.message = "An error occurred.";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        // DELETE: api/Approval/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Response result = new Response(true);
            try
            {
                var obj = _db.Approval.GetFirstOrDefault(x => x.Id == id);
                if (obj == null)
                {
                    result.status = false;
                    result.message = "Approval record not found.";
                }
                else
                {
                    _db.Approval.Remove(obj);
                    _db.Save();
                    result.message = "Approval record deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error deleting approval.";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        private void ProcessModuleApproval(ApprovalRefType refType, int refId, int userId)
        {
            switch (refType)
            {
                case ApprovalRefType.Transfer:
                    ProcessTransferApproval(refId, userId, TransferStatus.Approved);
                    break;
                case ApprovalRefType.Asset:
                    // Placeholder for Asset specific approval logic
                    break;
                case ApprovalRefType.Sale:
                    // Placeholder for Sale specific approval logic
                    break;
                default:
                    break;
            }
        }

        private void ProcessModuleReject(ApprovalRefType refType, int refId, int userId)
        {
            switch (refType)
            {
                case ApprovalRefType.Transfer:
                    ProcessTransferApproval(refId, userId, TransferStatus.Rejected);
                    break;
                case ApprovalRefType.Asset:
                    // Placeholder for Asset specific approval logic
                    break;
                case ApprovalRefType.Sale:
                    // Placeholder for Sale specific approval logic
                    break;
                default:
                    break;
            }
        }

        private void ProcessTransferApproval(int transferId, int approvedByUserId, TransferStatus status)
        {
            var transfer = _db.Transfer.GetFirstOrDefault(x => x.Id == transferId, "TransferDetails");
            if (transfer != null)
            {
                // Update the transfer record itself to reflect approval
                transfer.ApprovalBy = approvedByUserId;
                transfer.ModifiedBy = approvedByUserId;
                transfer.ModifiedDate = DateTime.Now;
                transfer.Status = status;
                _db.Transfer.Update(transfer);
                _db.Save();

                // Apply transfer to AssetStock only if approved
                if (status == TransferStatus.Approved)
                {
                    foreach (var detail in transfer.TransferDetails)
                    {
                        var asset = _db.AssetStock.GetFirstOrDefault(x => x.Id == detail.AssetStockId && !x.IsDeleted);
                        if (asset != null)
                        {
                            asset.BranchId = transfer.ToBranchId;
                            asset.LocationId = transfer.ToLocationId;
                            asset.CustodianId = transfer.ToCustodian;
                            asset.ModifiedDate = DateTime.Now;
                            asset.ModifiedBy = approvedByUserId;
                            _db.AssetStock.Update(asset);
                        }
                    }
                    _db.Save();
                }
            }
        }
    }
}
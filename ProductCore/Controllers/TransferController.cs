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
    public class TransferController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly int _branchId;

        public TransferController(IUnitOfWork db, IMapper mapper, IUserService userService)
        {
            _db = db;
            _mapper = mapper;
            _userService = userService;
            _branchId = _userService.GetBranchId();
        }

        // GET: api/Transfer
        [HttpGet]
        public async Task<IActionResult> Get(string? filter,
            string? search,
            string? sortBy = "TransferDate",
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

                IQueryable<Transfer> query = _db.Transfer.GetAllQueriable(null,
                    "TransferDetails.AssetStock.ItemMaster," +
                    "FromBranch,ToBranch,FromLocation,ToLocation," +
                    "FromCustodianUser,ToCustodianUser,ApprovedByUser," +
                    "CreatedByUser,ModifiedByUser", dynamicFilters: filters);

                if (!string.IsNullOrEmpty(search))
                {
                    query = QueryHelper.ApplySearch(
                        query,
                        search,
                        x => x.TransferNo,
                        x => x.Remarks!
                    );
                }

                int totalRecords = query.Count();

                query = QueryHelper.ApplySorting(query, sortBy!, sortDirection!);

                var pagedData = QueryHelper.ApplyPagination(query, pageNumber, pageSize).ToList();

                var dtoData = _mapper.Map<List<TransferDto>>(pagedData);

                var rejectedTransferIds = pagedData
                    .Where(x => x.Status == TransferStatus.Rejected)
                    .Select(x => x.Id)
                    .ToList();

                if (rejectedTransferIds.Any())
                {
                    var approvals = _db.Approval.GetAll(x => 
                        x.RefType == ApprovalRefType.Transfer && 
                        rejectedTransferIds.Contains(x.RefId)
                    ).ToList();

                    var approvalsMap = approvals
                        .GroupBy(x => x.RefId)
                        .ToDictionary(
                            g => g.Key, 
                            g => g.OrderByDescending(x => x.CreatedDate).Select(x => x.Remarks).FirstOrDefault()
                        );

                    foreach (var dto in dtoData)
                    {
                        if (dto.Status == TransferStatus.Rejected && approvalsMap.TryGetValue(dto.Id, out var remarks))
                        {
                            dto.ApprovalRemarks = remarks;
                        }
                    }
                }

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
                result.message = "Error retrieving transfers.";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        // GET: api/Transfer/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Response result = new Response(true, "");
            try
            {
                var obj = _db.Transfer.GetFirstOrDefault(
                    x => x.Id == id,
                    "TransferDetails,FromBranch,ToBranch,FromLocation,ToLocation,FromCustodianUser,ToCustodianUser,ApprovedByUser,CreatedByUser,ModifiedByUser,TransferDetails,TransferDetails.AssetStock,TransferDetails.AssetStock.ItemMaster"
                );

                if (obj == null)
                {
                    result.status = false;
                    result.message = "Transfer record not found.";
                }
                else
                {
                    result.data = _mapper.Map<TransferDto>(obj);
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error retrieving transfer details.";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        // GET: api/AssetStock/GetAssetsForTransfer
        [HttpGet("GetAssetsForTransfer")]
        public async Task<IActionResult> GetAssetsForTransfer(
            string? search,
            string? sortBy = "RFID",
            string? sortDirection = "asc",
            int pageNumber = 1,
            int pageSize = 10)
        {
            Response result = new Response(true, "");

            try
            {
                // We need active items from AssetStock, including ItemMaster and its relations.
                IQueryable<AssetStock> query = _db.AssetStock
                    .GetAllQueriable(x => !x.IsDeleted && x.Status == AssetStockStatus.Active && x.BranchId == _branchId,
                                     "Branch,Location,ItemMaster,ItemMaster.Category,ItemMaster.Manufacturer,ItemMaster.Unit,TransferDetails.Transfer")
                    .AsQueryable();

                // Search
                query = QueryHelper.ApplySearch(
                    query,
                    search,
                    x => x.SerialNo!,
                    x => x.RFID!,
                    x => x.ItemMaster.ItemName!,
                    x => x.ItemMaster.Code!
                );

                int totalRecords = query.Count();

                // Sorting
                query = QueryHelper.ApplySorting(
                    query,
                    sortBy!,
                    sortDirection!
                );

                // Pagination
                var pagedData = QueryHelper
                    .ApplyPagination(query, pageNumber, pageSize)
                    .ToList();

                // Map Entity → DTO
                var dtoData = _mapper.Map<List<AssetStockDto>>(pagedData);
                var locationsMap = _db.Location.GetAll(x => x.Status == true && !x.IsDeleted).ToDictionary(x => x.Id, x => x);
                LocationHelper.PopulateLocationNames(dtoData, locationsMap);
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
                result.message = "Error";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }

        // POST: api/Transfer
        [HttpPost]
        public IActionResult Post(TransferDto dto)
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
                Transfer obj = _mapper.Map<Transfer>(dto);
                if (obj.FromLocationId == 0)
                {
                    obj.FromLocationId = null;
                }

                _db.BeginTransaction();

                if (obj.Id > 0)
                {
                    // Update
                    var existingTransfer = _db.Transfer.GetFirstOrDefault(x => x.Id == obj.Id, "TransferDetails");
                    if (existingTransfer == null)
                    {
                        result.status = false;
                        result.message = "Transfer record not found.";
                        return Ok(result);
                    }

                    // Update fields
                    existingTransfer.TransferDate = obj.TransferDate;
                    existingTransfer.FromBranchId = obj.FromBranchId;
                    existingTransfer.FromLocationId = obj.FromLocationId;
                    existingTransfer.FromCustodian = obj.FromCustodian;
                    existingTransfer.ToBranchId = obj.ToBranchId;
                    existingTransfer.ToLocationId = obj.ToLocationId;
                    existingTransfer.ToCustodian = obj.ToCustodian;
                    existingTransfer.Remarks = obj.Remarks;
                    existingTransfer.ApprovalRequired = obj.ApprovalRequired;
                    existingTransfer.ApprovalBy = obj.ApprovalBy;
                    existingTransfer.ModifiedBy = currentUserId;
                    existingTransfer.ModifiedDate = DateTime.Now;

                    // Update Transfer Details collection
                    // Remove deleted details
                    foreach (var existingDetail in existingTransfer.TransferDetails.ToList())
                    {
                        if (!obj.TransferDetails.Any(d => d.AssetStockId == existingDetail.AssetStockId))
                        {
                            _db.TransferDetails.Remove(existingDetail);
                        }
                    }
                    // Add new details
                    foreach (var newDetail in obj.TransferDetails)
                    {
                        if (!existingTransfer.TransferDetails.Any(d => d.AssetStockId == newDetail.AssetStockId))
                        {
                            newDetail.TransferId = existingTransfer.Id;
                            _db.TransferDetails.Add(newDetail);
                        }
                    }

                    _db.Transfer.Update(existingTransfer);
                    _db.Save();

                    // Apply asset stock branch & location changes if approved
                    ApplyAssetStockTransferUpdates(existingTransfer.Id);

                    result.message = "Transfer updated successfully.";
                }
                else
                {
                    // Create
                    // Auto-generate TransferNo: TRF-yyyyMMdd-XXXX
                    string prefix = "TRF-" + DateTime.Today.ToString("yyyyMMdd") + "-";
                    var todayTransfers = _db.Transfer.GetAll(x => x.TransferNo.StartsWith(prefix));
                    int nextSeq = 1;
                    if (todayTransfers.Any())
                    {
                        var maxNo = todayTransfers.Select(x => x.TransferNo).OrderByDescending(x => x).FirstOrDefault();
                        if (maxNo != null && maxNo.Length > prefix.Length)
                        {
                            var suffix = maxNo.Substring(prefix.Length);
                            if (int.TryParse(suffix, out int currentSeq))
                            {
                                nextSeq = currentSeq + 1;
                            }
                        }
                    }
                    obj.TransferNo = prefix + nextSeq.ToString("D4");
                    obj.CreatedBy = currentUserId;
                    obj.CreatedDate = DateTime.Now;
                    obj.ModifiedBy = currentUserId;
                    obj.ModifiedDate = DateTime.Now;

                    _db.Transfer.Add(obj);
                    _db.Save();

                    // Apply asset stock branch & location changes if approved
                    ApplyAssetStockTransferUpdates(obj.Id);

                    result.message = "Transfer saved successfully.";
                }

                _db.Commit();
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "An error occurred.";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        private void ApplyAssetStockTransferUpdates(int transferId)
        {
            var transfer = _db.Transfer.GetFirstOrDefault(x => x.Id == transferId, "TransferDetails");
            if (transfer != null)
            {
                bool shouldApply = !transfer.ApprovalRequired;
                if (shouldApply)
                {
                    transfer.Status = TransferStatus.Approved;
                    _db.Transfer.Update(transfer);

                    foreach (var detail in transfer.TransferDetails)
                    {
                        var asset = _db.AssetStock.GetFirstOrDefault(x => x.Id == detail.AssetStockId && !x.IsDeleted);
                        if (asset != null)
                        {
                            asset.BranchId = transfer.ToBranchId;
                            asset.LocationId = transfer.ToLocationId;
                            asset.CustodianId = transfer.ToCustodian;
                            asset.ModifiedDate = DateTime.Now;
                            if (transfer.ModifiedBy.HasValue)
                            {
                                asset.ModifiedBy = transfer.ModifiedBy.Value;
                            }
                            _db.AssetStock.Update(asset);
                        }
                    }
                    _db.Save();
                }
            }
        }

        // DELETE: api/Transfer/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Response result = new Response(true);
            try
            {
                var obj = _db.Transfer.GetFirstOrDefault(x => x.Id == id, "TransferDetails");
                if (obj == null)
                {
                    result.status = false;
                    result.message = "Transfer record not found.";
                }
                else
                {
                    foreach (var detail in obj.TransferDetails.ToList())
                    {
                        _db.TransferDetails.Remove(detail);
                    }
                    _db.Transfer.Remove(obj);
                    _db.Save();

                    result.message = "Transfer deleted successfully.";
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error deleting transfer.";
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }
    }
}
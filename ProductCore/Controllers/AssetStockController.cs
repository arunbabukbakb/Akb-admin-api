using AutoMapper;
using System.Text.Json;
using Models;
using Data;
using Data.Repository.IRepository;
using Models;
using Models.DtoModels;
using Infrastructure;
using Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetStockController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly int _branchId;

        public AssetStockController(IUnitOfWork db, IMapper mapper, IUserService userService)
        {
            _db = db;
            _mapper = mapper;
            _userService = userService;
            _branchId = _userService.GetBranchId();
        }

        // GET: api/AssetStock
        [HttpGet]
        public async Task<IActionResult> Get(string? filter, string? search,
            string? sortBy = "SerialNo",
            string? sortDirection = "asc",
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

                IQueryable<AssetStock> query =
                    _db.AssetStock.GetAllQueriable(x => !x.IsDeleted && x.BranchId == _branchId, dynamicFilters: filters);

                // Search
                query = QueryHelper.ApplySearch(
                    query,
                    search,
                    x => x.SerialNo!,
                    x => x.RFID!
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


        // GET: api/AssetStock/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Response result = new Response(true);

            try
            {
                var obj = _db.AssetStock
                    .GetFirstOrDefault(x => x.Id == id && !x.IsDeleted && x.BranchId == _branchId);

                if (obj == null)
                {
                    result.status = false;
                    result.message = "Asset not found.";
                }
                else
                {
                    var dto = _mapper.Map<AssetStockDto>(obj);
                    if (dto.LocationId.HasValue)
                    {
                        var locationsMap = _db.Location.GetAll(x => x.Status == true && !x.IsDeleted).ToDictionary(x => x.Id, x => x);
                        LocationHelper.PopulateLocationNames(new[] { dto }, locationsMap);
                    }
                    result.data = dto;
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }


        // GET: api/AssetStock/GetAssets
        [HttpGet("GetAssets")]
        public async Task<IActionResult> GetAssets(
            string? search,
            string? sortBy = "ItemName",
            string? sortDirection = "asc",
            int pageNumber = 1,
            int pageSize = 10)
        {
            Response result = new Response(true);

            try
            {
                // Get ItemMasters query with related navigation data
                IQueryable<ItemMaster> query = _db.ItemMaster
                    .GetAllQueriable(x => !x.IsDeleted, "Category,Manufacturer");

                // Search
                query = QueryHelper.ApplySearch(
                    query,
                    search,
                    x => x.ItemName!,
                    x => x.Code!
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

                var locationsMap = _db.Location.GetAll(x => x.Status == true && !x.IsDeleted).ToDictionary(x => x.Id, x => x);

                // Create list for result
                var assetsWithItems = new List<ItemMasterWithAssetsDto>();

                foreach (var itemMaster in pagedData)
                {
                    // Get AssetStock records for this ItemMaster
                    var assets = _db.AssetStock
                        .GetAll(x => x.ItemMasterId == itemMaster.Id && !x.IsDeleted && x.BranchId == _branchId,"Custodian")
                        .ToList();

                    // Map to DTO
                    var itemWithAssetsDto = _mapper.Map<ItemMasterWithAssetsDto>(itemMaster);
                    var assetDtos = _mapper.Map<List<AssetStockDto>>(assets);
                    LocationHelper.PopulateLocationNames(assetDtos, locationsMap);
                    itemWithAssetsDto.Assets = assetDtos;

                    assetsWithItems.Add(itemWithAssetsDto);
                }

                result.data = assetsWithItems;

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

        // POST: api/AssetStock (Add / Update)
        [HttpPost]
        public IActionResult Post(AssetStockDto dto)
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

                AssetStock obj = _mapper.Map<AssetStock>(dto);

                // Automatically assign current login branchId
                if (obj.BranchId == 0)
                {
                    obj.BranchId = _branchId;
                }

                if (obj.Id > 0)
                {
                    // Update
                    obj.ModifiedDate = DateTime.Now;

                    _db.AssetStock.Update(obj);
                    _db.Save();
                    result.message = "Update successfully";
                }
                else
                {
                    // Duplicate SerialNo check
                    bool isDuplicate = _db.AssetStock.Any(x =>
                        x.Id == obj.Id &&
                        !x.IsDeleted && 
                        x.BranchId == _branchId);

                    if (isDuplicate)
                    {
                        result.status = false;
                        result.message = "Asset with same serial number already exists.";
                        return Ok(result);
                    }

                    obj.CreatedDate = DateTime.Now;
                    obj.ModifiedDate = DateTime.Now;

                    _db.AssetStock.Add(obj);
                    _db.Save();
                    result.message = "Saved successfully";
                }

             
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "An error occurred.";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }

        // POST: api/AssetStock/AssetTransfer
        [HttpPost("AssetTransfer")]
        public IActionResult AssetTransfer(List<AssetTransferDto> dtos)
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

                if (dtos == null || !dtos.Any())
                {
                    result.status = false;
                    result.message = "No assets provided for transfer.";
                    return Ok(result);
                }

                int transferCount = 0;

                foreach (var dto in dtos)
                {
                    var assetStock = _db.AssetStock
                        .GetFirstOrDefault(x => x.Id == dto.Id && !x.IsDeleted && x.BranchId == _branchId);

                    if (assetStock != null)
                    {
                        assetStock.BranchId = dto.BranchId;
                        assetStock.LocationId = dto.LocationId;
                        assetStock.ModifiedDate = DateTime.Now;

                        _db.AssetStock.Update(assetStock);
                        transferCount++;
                    }
                }

                _db.Save();
                result.message = $"{transferCount} asset(s) transferred successfully.";
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "An error occurred.";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }

        // DELETE: api/AssetStock/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Response result = new Response(true);

            try
            {
                var obj = _db.AssetStock
                    .GetFirstOrDefault(x => x.Id == id && x.BranchId == _branchId);

                if (obj == null)
                {
                    result.status = false;
                    result.message = "Asset not found.";
                }
                else
                {
                    obj.IsDeleted = true;
                    obj.ModifiedDate = DateTime.Now;

                    _db.AssetStock.Update(obj);
                    _db.Save();

                    result.message = "Deleted successfully";
                }
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }
    }
}
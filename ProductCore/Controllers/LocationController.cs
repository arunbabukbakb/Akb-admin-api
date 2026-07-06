using AutoMapper;
using System.Text.Json;
using Models;
using Data.Repository.IRepository;
using Models;
using Models.DtoModels;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IMapper _mapper;

        public LocationController(IUnitOfWork db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET: api/Location
        [HttpGet]
        public IActionResult Get(string? filter, string? search,
            string? sortBy = "Location_Name",
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

                IQueryable<Location> query = _db.Location
                    .GetAll(x => x.Status==true)
                    .AsQueryable();

                // 🔍 Search
                query = QueryHelper.ApplySearch(
                    query,
                    search,
                    x => x.Name,
                    x => x.Code
                );

                // 🔢 Total count
                int totalRecords = query.Count();

                // ↕ Sorting
                query = QueryHelper.ApplySorting(
                    query,
                    sortBy!,
                    sortDirection!
                );

                // 📄 Pagination
                var pagedData = QueryHelper
                    .ApplyPagination(query, pageNumber, pageSize)
                    .ToList();

                result.data = pagedData;

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

        // GET: api/Location/GetByBranch/5
        [HttpGet("GetByBranch/{branchId}")]
        public IActionResult GetByBranch(int branchId)
        {
            Response result = new Response(true, "");

            try
            {
                var locationIds = _db.LocationBranch
                    .GetAll(x => x.BranchId == branchId && !x.IsDeleted)
                    .Select(x => (long)x.LocationId)
                    .ToList();

                var locations = _db.Location
                    .GetAll(x => locationIds.Contains(x.Id) && x.Status == true)
                    .ToList();

                result.data = locations;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }

        // GET: api/Location/5
        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            Response result = new Response(true, "");

            try
            {
                var location = _db.Location.GetFirstOrDefault(
                    x => x.Id == id && x.Status == true);

                if (location == null)
                {
                    result.status = false;
                    result.message = "Location not found.";
                }
                else
                {
                    result.data = location;
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

        // POST: api/Location (Add / Update)
        [HttpPost]
        public IActionResult Post(LocationDto dto)
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

                if (dto.Id > 0)
                {
                    // 🔄 Update
                    var existing = _db.Location.GetFirstOrDefault(x => x.Id == dto.Id);

                    if (existing == null)
                    {
                        result.status = false;
                        result.message = "Location not found.";
                        return Ok(result);
                    }

                    // 🔁 Map DTO changes onto the tracked entity
                    _mapper.Map(dto, existing);

                    // Set audit fields
                    existing.ModifiedDate = DateTime.Now;
                    existing.ModifiedBy = 0;

                    _db.Location.Update(existing);
                    _db.Save();
                }
                else
                {
                    // 🔁 Map DTO → Entity for creation
                    Location obj = _mapper.Map<Location>(dto);

                    // 🔁 Duplicate check (Location_Code)
                    if (!string.IsNullOrWhiteSpace(obj.Code))
                    {
                        bool isDuplicate = _db.Location.Any(x =>
                            x.Code == obj.Code &&
                            x.Status == true);

                        if (isDuplicate)
                        {
                            result.status = false;
                            result.message = "Location code already exists.";
                            return Ok(result);
                        }
                    }

                    // Set audit fields
                    obj.CreatedDate = DateTime.Now;
                    obj.CreatedBy = /* currentUserId */ 0;

                    _db.Location.Add(obj);
                    _db.Save();
                }

                result.message = "Saved successfully";
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "An error occurred during the operation.";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }

        // DELETE: api/Location/Delete/5 (Soft Delete)
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            Response result = new Response(true);

            try
            {
                var obj = _db.Location.GetFirstOrDefault(
                    x => x.Id == id && x.Status==true);

                if (obj == null)
                {
                    result.status = false;
                    result.message = "Location not found.";
                }
                else
                {
                    obj.Status = false;

                    _db.Location.Update(obj);
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
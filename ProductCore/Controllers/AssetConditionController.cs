using AutoMapper;
using System.Text.Json;
using Models;
using Data.Repository.IRepository;
using Models;
using Models.DtoModels;
using Models.ViewModels;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetConditionController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IMapper _mapper;

        public AssetConditionController(IUnitOfWork db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<IActionResult> Get(string? filter, string? search,
            string? sortBy = "Name",
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

                IQueryable<AssetCondition> query = _db.AssetCondition.GetAll(x => !x.IsDeleted, dynamicFilters: filters)
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

                // ✅ Pagination metadata
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

        // GET: api/Product/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Response result = new Response(true, "");

            try
            {
                var product = _db.AssetCondition.GetFirstOrDefault(
                    x => x.Id == id && !x.IsDeleted);

                if (product == null)
                {
                    result.status = false;
                    result.message = "Asset Condition not found.";
                }
                else
                {
                    result.data = product;
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

        [HttpPost]
        public IActionResult Post(AssetConditionDto dto)
        {
            var result = new Response(true);

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new Response(false)
                    {
                        message = "Model state is invalid."
                    });
                }

                if (dto.Id > 0)
                {
                    // 🔄 UPDATE
                    var existing = _db.AssetCondition
                        .GetFirstOrDefault(x => x.Id == dto.Id && !x.IsDeleted);

                    if (existing == null)
                    {
                        return NotFound(new Response(false)
                        {
                            message = "Item not found."
                        });
                    }

                    // 🔁 Duplicate check (exclude current record)
                    bool isDuplicate = _db.AssetCondition.Any(x =>
                        x.Name == dto.Name &&
                        x.Id != dto.Id &&
                        !x.IsDeleted);

                    if (isDuplicate)
                    {
                        return Conflict(new Response(false)
                        {
                            message = "Item Name already exists."
                        });
                    }

                    _mapper.Map(dto, existing);
                    existing.ModifiedDate = DateTime.Now;


                    _db.AssetCondition.Update(existing);

                    result.message = "Updated successfully";
                }
                else
                {
                    // ➕ ADD
                    bool isDuplicate = _db.AssetCondition.Any(x =>
                        x.Name == dto.Name &&
                        !x.IsDeleted);

                    if (isDuplicate)
                    {
                        return Conflict(new Response(false)
                        {
                            message = "Item Name already exists."
                        });
                    }

                    var obj = _mapper.Map<AssetCondition>(dto);

                    obj.CreatedDate = DateTime.Now;
                    obj.IsDeleted = false;
                    obj.Status = true;

                    _db.AssetCondition.Add(obj);

                    result.message = "Saved successfully";
                }

                _db.Save();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // 🔴 Log error here (important)
                // _logger.LogError(ex, "Error in AssetCondition Post");

                return StatusCode(500, new Response(false)
                {
                    message = "An unexpected error occurred.",
                    systemMessage = ex.Message // remove in production if sensitive
                });
            }
        }

        // DELETE: api/Product/Delete/5 (Soft Delete)
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Response result = new Response(true);

            try
            {
                var obj = _db.AssetCondition.GetFirstOrDefault(
                    x => x.Id == id && !x.IsDeleted);

                if (obj == null)
                {
                    result.status = false;
                    result.message = "Item not found.";
                }
                else
                {
                    obj.IsDeleted = true;
                    obj.ModifiedDate = DateTime.Now;

                    _db.AssetCondition.Update(obj);
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
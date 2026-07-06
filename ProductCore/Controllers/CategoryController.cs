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
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IMapper _mapper;

        public CategoryController(IUnitOfWork db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET: api/CategoryMaster
        [HttpGet]
        public async Task<IActionResult> Get(string? filter, string? search,
            string? sortBy = "AssetCategoryName",
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

                IQueryable<Category> query =
                    _db.Category.GetAllQueriable(dynamicFilters: filters);

                // 🔍 Search
                query = QueryHelper.ApplySearch(
                    query,
                    search,
                    x => x.Name,
                    x => x.Code,
                    x => x.ShortCode
                );

                // 🔢 Total count BEFORE pagination
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

                // ✅ Assign response data
                result.data = pagedData;

                // ✅ Assign pagination metadata
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


        // GET: api/CategoryMaster/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Response result = new Response(true, "");
            try
            {
                var category = _db.Category
                    .GetById(x => x.Id == id);

                if (category == null)
                {
                    result.status = false;
                    result.message = "Category not found.";
                }
                else
                {
                    result.data = category;
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

        // POST: api/CategoryMaster (Add / Update)
        [HttpPost]
        public IActionResult Post(CategoryDto dto)
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

                // Map DTO → Entity
                Category obj = _mapper.Map<Category>(dto);

                if (obj.Id > 0)
                {
                    // Update
                    _db.Category.Update(obj);
                    _db.Save();
                }
                else
                {
                    // Duplicate check (Code / Name)
                    bool isDuplicate = _db.Category.Any(c =>
                        c.Code == obj.Code ||
                        c.Name == obj.Name);

                    if (isDuplicate)
                    {
                        result.status = false;
                        result.message = "Category already exists.";
                        return Ok(result);
                    }

                    obj.CreatedDate = DateTime.Now;
                    _db.Category.Add(obj);
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


        // DELETE: api/CategoryMaster/Delete/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Response result = new Response(true);
            try
            {
                var obj = _db.Category
                    .GetFirstOrDefault(c => c.Id == id);

                if (obj == null)
                {
                    result.status = false;
                    result.message = "Category not found.";
                }
                else
                {
                    _db.Category.Remove(obj);
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
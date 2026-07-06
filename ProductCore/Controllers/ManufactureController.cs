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
    public class ManufactureController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IMapper _mapper;

        public ManufactureController(IUnitOfWork db, IMapper mapper)
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

                IQueryable<Manufacturer> query = _db.Manufacture.GetAll(x => !x.IsDeleted, dynamicFilters: filters)
                    .AsQueryable();

                // 🔍 Search
                query = QueryHelper.ApplySearch(
                    query,
                    search,
                    x => x.Name,
                    x => x.Code,
                    x => x.PhoneNumber
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
                var product = _db.Manufacture.GetFirstOrDefault(
                    x => x.Id == id && !x.IsDeleted);

                if (product == null)
                {
                    result.status = false;
                    result.message = "Product not found.";
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

        // POST: api/Product (Add / Update)
        [HttpPost]
        public IActionResult Post(ManufacturerDto dto)
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

                Manufacturer obj = _mapper.Map<Manufacturer>(dto);

                if (obj.Id > 0)
                {
                    // 🔄 Update
                    var existing = _db.Manufacture.GetFirstOrDefault(
                        x => x.Id == obj.Id && !x.IsDeleted);

                    if (existing == null)
                    {
                        result.status = false;
                        result.message = "Item not found.";
                        return Ok(result);
                    }

                    // Preserve system fields
                    obj.CreatedBy = existing.CreatedBy;
                    obj.CreatedDate = existing.CreatedDate;
                    obj.IsDeleted = existing.IsDeleted;

                    obj.ModifiedDate = DateTime.Now;

                    _db.Manufacture.Update(obj);
                }
                else
                {
                    // 🔁 Duplicate check (ProductCode)
                    bool isDuplicate = _db.Manufacture.Any(p =>
                        p.Name == obj.Name &&
                        !p.IsDeleted);

                    if (isDuplicate)
                    {
                        result.status = false;
                        result.message = "Item Name already exists.";
                        return Ok(result);
                    }

                    obj.CreatedDate = DateTime.Now;
                    obj.IsDeleted = false;

                    _db.Manufacture.Add(obj);
                }

                _db.Save();
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


        // DELETE: api/Product/Delete/5 (Soft Delete)
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Response result = new Response(true);

            try
            {
                var obj = _db.Manufacture.GetFirstOrDefault(
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

                    _db.Manufacture.Update(obj);
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
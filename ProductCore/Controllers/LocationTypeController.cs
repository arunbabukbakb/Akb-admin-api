using Models;
using System.Text.Json;
using Data.Repository.IRepository;
using Models;
using Models.DtoModels;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UserCore.Controllers
{
	//[Authorize(Roles = "admin,manager")]
	[Route("api/[controller]")]
	[ApiController]
	public class LocationTypeController : ControllerBase
	{
		private readonly IUnitOfWork _db;
		private readonly IUserService _userService;

		public LocationTypeController(IUnitOfWork db, IUserService userService)
		{
			_db = db;
			_userService = userService;
		}

        [HttpGet]
        public IActionResult Get(string? filter)
        {
            Response result = new Response(true, "");
            try
            {
                Dictionary<string, FilterCondition>? filters = null;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filters = JsonSerializer.Deserialize<Dictionary<string, FilterCondition>>(filter, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                var dataList = _db.LocationType.GetAll(dynamicFilters: filters);
                result.data = dataList;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post(LocationType obj)
        {
            Response result = new Response(true);

            try
            {
                if (!ModelState.IsValid)
                {
                    result.status = false;
                    result.message = "Invalid data";
                    return Ok(result);
                }

                // 🔴 Check duplicate Role NAME
                bool isNameDuplicate = _db.LocationType.Any(r =>
                    r.Name.ToLower().Trim() == obj.Name.ToLower().Trim()
                    && (obj.Id == 0 || r.Id != obj.Id));

                if (isNameDuplicate)
                {
                    result.status = false;
                    result.message = "Name already exists.";
                    return Ok(result);
                }

                // ✅ Create / Update
                if (obj.Id == 0)
                {
                    _db.LocationType.Add(obj);
                    result.message = "Created successfully";
                }
                else
                {
                    _db.LocationType.Update(obj);
                    result.message = "Updated successfully";
                }

                _db.Save();
            }
            catch (Exception ex)
            {
                result.status = false;
                result.message = "Error";
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }


        [HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			Response result = new Response(true);
			try
			{
				var obj = _db.LocationType.GetFirstOrDefault(a => a.Id == id);
				if (obj == null)
				{
					result.status = false;
					result.message = "Error while deleting";
				}
				else
				{
					result.message = "Deleted successfully";
					_db.LocationType.Remove(obj);
					_db.Save();
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
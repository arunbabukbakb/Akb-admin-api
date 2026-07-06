using Models;
using System.Text.Json;
using Data.Repository.IRepository;
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
	public class RoleController : ControllerBase
	{
		private readonly IUnitOfWork _db;
		private readonly IUserService _userService;

		public RoleController(IUnitOfWork db, IUserService userService)
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

                var dataList = _db.Role.GetAll(dynamicFilters: filters);
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
        public IActionResult Post(Role obj)
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
                bool isNameDuplicate = _db.Role.Any(r =>
                    r.Name.ToLower().Trim() == obj.Name.ToLower().Trim()
                    && (obj.Id == 0 || r.Id != obj.Id));

                if (isNameDuplicate)
                {
                    result.status = false;
                    result.message = "Role name already exists.";
                    return Ok(result);
                }

                //Set codename 
                obj.CodeName = obj.Name.ToLower().Trim();

                // 🔴 Check duplicate Role CODE (THIS WAS MISSING)
                bool isCodeDuplicate = _db.Role.Any(r =>
                    r.CodeName == obj.CodeName
                    && (obj.Id == 0 || r.Id != obj.Id));

                if (isCodeDuplicate)
                {
                    result.status = false;
                    result.message = "Role code already exists.";
                    return Ok(result);
                }

                // ✅ Create / Update
                if (obj.Id == 0)
                {
                    _db.Role.Add(obj);
                    result.message = "Role created successfully";
                }
                else
                {
                    _db.Role.Update(obj);
                    result.message = "Role updated successfully";
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
				var obj = _db.Role.GetFirstOrDefault(a => a.Id == id);
				if (obj == null)
				{
					result.status = false;
					result.message = "Error while deleting";
				}
				else
				{
					result.message = "Deleted successfully";
					_db.Role.Remove(obj);
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
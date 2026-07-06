using AutoMapper;
using System.Text.Json;
using Models;
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
	public class UnitController : ControllerBase
	{
		private readonly IUnitOfWork _db;
		private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UnitController(IUnitOfWork db, IUserService userService, IMapper mapper)
		{
			_db = db;
			_userService = userService;
            _mapper = mapper;
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

                var dataList = _db.Unit.GetAll(dynamicFilters: filters);
                var dtoList = _mapper.Map<List<UnitDto>>(dataList);
                result.data = dtoList;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post(Unit obj)
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
                bool isNameDuplicate = _db.Unit.Any(r =>
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
                    _db.Unit.Add(obj);
                    result.message = "Created successfully";
                }
                else
                {
                    _db.Unit.Update(obj);
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
				var obj = _db.Unit.GetFirstOrDefault(a => a.Id == id);
				if (obj == null)
				{
					result.status = false;
					result.message = "Error while deleting";
				}
				else
				{
					result.message = "Deleted successfully";
					_db.Unit.Remove(obj);
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
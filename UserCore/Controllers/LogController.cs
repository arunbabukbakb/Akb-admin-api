
using Models;
using Models.DtoModels;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Data.Repository.IRepository;

namespace UserCore.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
   
    //[Authorize(Roles = "Admin,SP Manager")]
    public class LogController : ControllerBase
	{
		private readonly IUnitOfWork _db;
		private readonly IUserService _userService;

		public LogController(IUnitOfWork db, IUserService userService)
		{
			_db = db;
			_userService = userService;
		}

		[HttpGet("{id}")]
		public IActionResult UserTokenCheck(int id)
		{
			return Ok();
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

				var Product = _db.UserLog.GetAll(null, "User", dynamicFilters: filters);
				result.data = Product;				
			}
			catch (Exception ex)
			{
				result.status = false;
				result.message = "Error";
				result.systemMessage = ex.Message;
			}			
			return Ok(result);
		}

		[HttpGet("getuserlog")]
		public IActionResult GetUserLogs(int userid, int page = 1, int pageSize = 10, string? sortBy = null, string? sortOrder = null, string? search = null)
		{
			Response result = new Response(true, "");
			try
			{
				Expression<Func<UserLog, bool>> conditon = a => a.UserId == userid &&
					(EF.Functions.Like(a.LogMessage ?? "", $"%{search}%") ||
					(a.User != null && EF.Functions.Like(a.User.NickName ?? "", $"%{search}%")) ||
					EF.Functions.Like(a.Timestamp.ToString(), $"%{search}%"));

				var datas = _db.UserLog.GetAllPaginated(page, pageSize, conditon, "User", sortBy, sortOrder);
				result.data = datas;
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
				var obj = _db.UserLog.GetFirstOrDefault(a => a.LogId == id);
				if (obj == null)
				{
					result.status = false;
					result.message = "Error while deleting";
				}
				else
				{
					result.message = "Deleted successfully";
					_db.UserLog.Remove(obj);
					_db.Save();
				}
			}
			catch(Exception ex)
			{
				result.status = false;
				result.message = "Error";
				result.systemMessage = ex.Message;
			}
			return Ok(result);
		}
	}
}
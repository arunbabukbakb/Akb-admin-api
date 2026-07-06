using Models;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Data.Repository
{
	public class UserService : IUserService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private ApplicationDbContext _db;

		public UserService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
		{
			_httpContextAccessor = httpContextAccessor;
			_db = db;
		}

		public int GetUserId()
		{
			int thisuserId = 0;
			// Retrieve the user's claims from the authenticated user's identity
			var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if(userId != null)
			{
				thisuserId = int.Parse(userId.ToString());
			}
			return thisuserId;
		}

		public int GetBranchId()
		{
			int thisbranchId = 0;
			// Retrieve the BranchId from the authenticated user's claims
			var branchId = _httpContextAccessor.HttpContext?.User?.FindFirst("BranchId")?.Value;
			if(branchId != null)
			{
				thisbranchId = int.Parse(branchId.ToString());
			}
			return thisbranchId;
		}

		public int GetCompanyId()
		{
			int thiscompanyId = 0;
			// Retrieve the CompanyId from the authenticated user's claims
			var companyId = _httpContextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;
			if(companyId != null)
			{
				thiscompanyId = int.Parse(companyId.ToString());
			}
			return thiscompanyId;
		}

		public void LogInsert(string level, string message)
		{
			int thisuserId = GetUserId();
			var obj = new UserLog
			{
				UserId = thisuserId,
				LogLevel = level,
				LogMessage = message
			};

			_db.UserLog.Add(obj);
			_db.SaveChanges();
		}
	}
}

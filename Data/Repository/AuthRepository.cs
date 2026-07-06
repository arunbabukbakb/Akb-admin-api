using Models;
using Data.Repository.IRepository;
using Models.DtoModels;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Data.Repository
{
	public class AuthRepository : IAuthRepository
	{
		private readonly IUnitOfWork _db;
		private readonly ISettingsRepository _setting;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthRepository(IUnitOfWork db, ISettingsRepository settings, IHttpContextAccessor httpContextAccessor)
		{
			_db = db;
			_setting = settings;
            _httpContextAccessor = httpContextAccessor;
        }
		public Users? Authenticate(LoginModel login)
		{
			string includeproperty = (login.UserName == "admin" ? "Role" : "Role,UserBranch.Branch");
			var user = _db.User.GetFirstOrDefault(a => a.UserName == login.UserName, includeproperty);
			if (user != null)
			{
				if (!_setting.VerifyPassword(login.Password ?? "", user.Password ?? ""))
				{
					user = null;
				}
			}

			return user;
		}

		public Customer? CustomerAuthenticate(LoginModel login)
		{
			var user = _db.Customer.GetFirstOrDefault(a => a.UserName == login.UserName);
			if (user != null)
			{
				if (!_setting.VerifyPassword(login.Password ?? "", user.Password ?? ""))
				{
					user = null;
				}
			}

			return user;
		}

        public LoggedInUserDto GetLoggedInUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("User not authenticated");

            return new LoggedInUserDto
            {
                UserId = int.Parse(
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"
                ),
                RoleId = int.Parse(
                    user.FindFirst(ClaimTypes.Sid)?.Value ?? "0"
                ),
                RoleName = user.FindFirst(ClaimTypes.Role)?.Value ?? ""
            };
        }
    }
}

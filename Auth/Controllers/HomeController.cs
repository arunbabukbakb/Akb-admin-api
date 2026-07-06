
using Models;
using Models.ViewModels;
using Data.Repository.IRepository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Auth.Controllers
{
    //[Authorize(Roles ="admin")]
    //[CustomAuthorize(roles: "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IJWTManagerRepository _jWTManager;
        private readonly IAuthRepository authrepo;
        private readonly ILogger<HomeController> _logger;
        private readonly ISettingsRepository _setting;

        public HomeController(IJWTManagerRepository jWTManager, IAuthRepository authrepo, IUnitOfWork db, ILogger<HomeController> logger, ISettingsRepository setting)
        {
            this._jWTManager = jWTManager;
            this.authrepo = authrepo;
            _db = db;
            _logger = logger;
            _setting = setting;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public IActionResult Authenticate(LoginModel login)
        {
            Response result = new Response(true);
            try
            {
                //var user = authrepo.Authenticate(login);
                string includeproperty = (login.UserName == "admin" ? "Role" : "Role,UserBranches.Branch");
                var user = _db.User.GetFirstOrDefault(a => a.UserName == login.UserName, includeproperty);
                if (user != null)
                {
                    if (user.Status == false)
                    {
                        user = null;
                        result.status = false;
                        result.message = "Not Approved";
                    }
                    else if (!_setting.VerifyPassword(login.Password ?? "", user.Password ?? ""))
                    {
                        user = null;
                        result.status = false;
                        result.message = "Invalid Credentials";
                    }
                }
                else
                {
                    result.status = false;
                    result.message = "Username doesn't exist.";
                }

                if (user != null)
                {
                    var branchId = _db.UserBranch.GetFirstOrDefault(x => x.UserId == user.Id && x.IsDefault)?.BranchId;
                    var token = _jWTManager.Authenticate(user.Role?.CodeName, user.Id, user.Role?.Id, branchId, user?.CompanyId);
                    token.User = user;

                    var company = _db.Company.GetFirstOrDefault(a => a.Id > 0);
                    if (company != null)
                    {
                        token.Logo = company.Logo;
                    }

                    if (token == null)
                    {
                        return Unauthorized();
                    }
                    result.data = token;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                result.status = false;
                result.message = ex.Message;
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("regenerate-token")]
        public IActionResult RegenerateToken(BranchChangeTokenRequest request)
        {
            Response result = new Response(true);
            try
            {
                if (request.BranchId <= 0)
                {
                    result.status = false;
                    result.message = "BranchId is required.";
                    return BadRequest(result);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var user = _db.User.GetFirstOrDefault(x => x.Id == userId, "Role");
                if (user == null)
                {
                    return Unauthorized();
                }

                var branch = _db.Branch.GetFirstOrDefault(x => x.Id == request.BranchId);
                if (branch == null)
                {
                    result.status = false;
                    result.message = "Branch not found.";
                    return NotFound(result);
                }

                var canUseBranch = user.Role?.CodeName == "admin" || _db.UserBranch.Any(x => x.UserId == userId && x.BranchId == request.BranchId);
                if (!canUseBranch)
                {
                    result.status = false;
                    result.message = "User does not have access to the requested branch.";
                    return Forbid();
                }

                var token = _jWTManager.Authenticate(user.Role?.CodeName, user.Id, user.Role?.Id, request.BranchId, user?.CompanyId);
                token.User = user;

                var company = _db.Company.GetFirstOrDefault(a => a.Id > 0);
                if (company != null)
                {
                    token.Logo = company.Logo;
                }
                result.data = token;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                result.status = false;
                result.message = ex.Message;
            }

            return Ok(result);
        }
    }
}

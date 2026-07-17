
using Models;
using Models.ViewModels;
using Data.Repository.IRepository;
using Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;

        public HomeController(IJWTManagerRepository jWTManager, IAuthRepository authrepo, IUnitOfWork db, ILogger<HomeController> logger, ISettingsRepository setting, ApplicationDbContext context)
        {
            this._jWTManager = jWTManager;
            this.authrepo = authrepo;
            _db = db;
            _logger = logger;
            _setting = setting;
            _context = context;
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
                    
                    // Save refresh token to user in database
                    user.RefreshToken = token.RefreshToken;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                    _db.User.Update(user);
                    _db.Save();

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
                
                // Save refresh token to user in database
                user.RefreshToken = token.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                _db.User.Update(user);
                _db.Save();

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

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh")]
        public IActionResult Refresh(TokenApiModel tokenApiModel)
        {
            Response result = new Response(true);
            try
            {
                if (tokenApiModel == null || string.IsNullOrEmpty(tokenApiModel.AccessToken) || string.IsNullOrEmpty(tokenApiModel.RefreshToken))
                {
                    result.status = false;
                    result.message = "Invalid client request";
                    return BadRequest(result);
                }

                string accessToken = tokenApiModel.AccessToken;
                string refreshToken = tokenApiModel.RefreshToken;

                var principal = _jWTManager.GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    result.status = false;
                    result.message = "Invalid access token";
                    return BadRequest(result);
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    result.status = false;
                    result.message = "Invalid token claims";
                    return BadRequest(result);
                }

                var user = _db.User.GetFirstOrDefault(u => u.Id == userId, "Role");
                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    result.status = false;
                    result.message = "Invalid refresh token or token expired";
                    return BadRequest(result);
                }

                if (user.Status == false)
                {
                    result.status = false;
                    result.message = "User is not active/approved";
                    return BadRequest(result);
                }

                var branchId = _db.UserBranch.GetFirstOrDefault(x => x.UserId == user.Id && x.IsDefault)?.BranchId;
                var branchIdClaim = principal.FindFirst("BranchId")?.Value;
                if (int.TryParse(branchIdClaim, out int tokenBranchId) && tokenBranchId > 0)
                {
                    branchId = tokenBranchId;
                }

                var newTokens = _jWTManager.Authenticate(user.Role?.CodeName, user.Id, user.Role?.Id, branchId, user?.CompanyId);

                // Update refresh token in DB
                user.RefreshToken = newTokens.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                _db.User.Update(user);
                _db.Save();

                newTokens.User = user;
                var company = _db.Company.GetFirstOrDefault(a => a.Id > 0);
                if (company != null)
                {
                    newTokens.Logo = company.Logo;
                }

                result.data = newTokens;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred in refresh token: {ex.Message}");
                result.status = false;
                result.message = ex.Message;
            }

            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("reset-database")]
        public IActionResult ResetDatabase()
        {
            Response result = new Response(true);
            try
            {
                // Set command timeout to 60 seconds for migrations/seeding
                _context.Database.SetCommandTimeout(60);

                // Delete the database schema
                _context.Database.EnsureDeleted();

                // Re-run migrations and seeds
                _context.Database.Migrate();

                // Run DbInitializer checks
                DbInitializer.Initialize(_context);

                result.message = "Database reset to defaults successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during database reset: {ex.Message}");
                result.status = false;
                result.message = ex.Message;
            }

            return Ok(result);
        }
    }
}

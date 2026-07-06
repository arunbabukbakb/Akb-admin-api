using Models;
using Data.Repository.IRepository;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MenuPermissionController : ControllerBase
    {
        private readonly IUnitOfWork _db;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly IJWTManagerRepository _jWTManager;
        private readonly ISettingsRepository _setting;
        private readonly IAuthRepository authrepo;

        public MenuPermissionController(IUnitOfWork db, IWebHostEnvironment hostEnv, IJWTManagerRepository jWTManager, ISettingsRepository setting, IAuthRepository authrepo)
        {
            _db = db;
            _hostEnv = hostEnv;
            _jWTManager = jWTManager;
            _setting = setting;
            this.authrepo = authrepo;
        }

        [HttpGet("by-role/{roleId}")]
        public IActionResult GetByRole(int roleId)
        {
            Response result = new Response(true);

            try
            {
                var menus = _db.Menu.GetAll();   // Menu master
                var permissions = _db.MenuPermission
                                     .GetAll(x => x.RoleId == roleId);

                var data = from m in menus
                           join p in permissions
                           on m.Id equals p.MenuId into mp
                           from perm in mp.DefaultIfEmpty()
                           select new MenuPermission
                           {
                               Id = perm != null ? perm.Id : 0,
                               RoleId = roleId,
                               MenuId = m.Id,
                               MenuName = m.Title,
                               CanView = perm?.CanView ?? false,
                               CanAdd = perm?.CanAdd ?? false,
                               CanEdit = perm?.CanEdit ?? false,
                               CanDelete = perm?.CanDelete ?? false,
                               ShowInHome = perm?.ShowInHome ?? false
                           };

                result.data = data.OrderBy(x => x.MenuName).ToList();
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("my-menus")]
        public IActionResult GetMyMenus()
        {
            Response result = new Response(true);

            try
            {
                var user = authrepo.GetLoggedInUser();
                bool isAdmin = user.RoleName.Equals("admin", StringComparison.OrdinalIgnoreCase);

                // 🔹 Step 1: Get flat menu list
                var menus = isAdmin
                    ? _db.Menu
                        .GetAll(x => x.Status == true)
                        .Select(m => new
                        {
                            m.Id,
                            m.Title,
                            m.Path,
                            m.Icon,
                            m.OrderNumber,
                            m.ParentMenuId,
                            CanView = true,
                            CanAdd = true,
                            CanEdit = true,
                            CanDelete = true,
                            ShowInHome = true
                        })
                        .ToList()
                    : _db.MenuPermission
                        .GetAll(
                            x => x.RoleId == user.RoleId &&
                                (x.CanView || x.CanAdd || x.CanEdit || x.CanDelete || x.ShowInHome),
                            includeProperties: "Menu"
                        )
                        .Where(x => x.Menu!.Status == true)
                        .Select(x => new
                        {
                            Id = x.MenuId,
                            x.Menu!.Title,
                            x.Menu!.Path,
                            x.Menu!.Icon,
                            x.Menu!.OrderNumber,
                            x.Menu!.ParentMenuId,
                            x.CanView,
                            x.CanAdd,
                            x.CanEdit,
                            x.CanDelete,
                            x.ShowInHome
                        })
                        .ToList();

                // 🔹 Step 2: Build parent → child tree
                var menuTree = menus
                    .Where(x => x.ParentMenuId == null)
                    .OrderBy(x => x.OrderNumber)
                    .Select(parent => new
                    {
                        MenuId = parent.Id,
                        parent.Title,
                        parent.Path,
                        parent.Icon,
                        parent.OrderNumber,
                        parent.CanView,
                        parent.CanAdd,
                        parent.CanEdit,
                        parent.CanDelete,
                        parent.ShowInHome,
                        Children = menus
                            .Where(c => c.ParentMenuId == parent.Id)
                            .OrderBy(c => c.OrderNumber)
                            .Select(child => new
                            {
                                MenuId = child.Id,
                                child.Title,
                                child.Path,
                                child.Icon,
                                child.OrderNumber,
                                child.CanView,
                                child.CanAdd,
                                child.CanEdit,
                                child.CanDelete,
                                child.ShowInHome
                            })
                            .ToList()
                    })
                    .ToList();

                result.data = menuTree;
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }


        [HttpPost("save")]
        public IActionResult SavePermissions([FromBody] List<MenuPermission> permissions)
        {
            Response result = new Response(true);

            try
            {
                if (permissions == null || !permissions.Any())
                {
                    result.status = false;
                    result.message = "No permissions received";
                    return Ok(result);
                }

                int roleId = permissions.First().RoleId;

                // Remove existing permissions for role
                var existing = _db.MenuPermission.GetAll(x => x.RoleId == roleId).ToList();
                _db.MenuPermission.RemoveRange(existing);

                // Add only valid permissions
                var validPermissions = permissions.Where(x =>
                    x.CanView || x.CanAdd || x.CanEdit || x.CanDelete || x.ShowInHome
                ).ToList();

                if (validPermissions.Any())
                {
                    _db.MenuPermission.AddRange(validPermissions);
                }

                _db.Save();
                result.message = "Permissions saved successfully";
            }
            catch (Exception ex)
            {
                result.status = false;
                result.systemMessage = ex.Message;
            }

            return Ok(result);
        }


    }
}

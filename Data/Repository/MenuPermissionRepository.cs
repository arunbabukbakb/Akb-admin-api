using Models;
using Data.Repository.IRepository;

namespace Data.Repository
{
    public class MenuPermissionRepository : Repository<MenuPermission>, IMenuPermissionRepository
    {
        private ApplicationDbContext _db;
        public MenuPermissionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(MenuPermission obj)
        {
            _db.MenuPermissions.Update(obj);
        }

        public IEnumerable<MenuPermission> GetAllMenusPermission(int roleId)
        {
            var menusWithPermissions = from m in _db.Menus.Where(a => a.Status == true)
                                       join p in _db.MenuPermissions
                                            on new { MenuId = m.Id, RoleId = roleId } equals new { MenuId = p.MenuId, RoleId = p.RoleId }
                                            into permissions
                                       from permission in permissions.DefaultIfEmpty()
                                       select new MenuPermission
                                       {
                                           Menu = new Menu
                                           {
                                               Id = m.Id,
                                               Title = m.Title,
                                               Path = m.Path
                                           },
                                           MenuName =m.Path,
                                           Id = m.Id,
                                           MenuId = m.Id,
                                           RoleId = roleId,
                                           CanView = permission.CanView ? true : false,
                                           CanAdd = permission.CanAdd ? true : false,
                                           CanDelete = permission.CanDelete ? true : false,
                                           CanEdit = permission.CanEdit ? true : false,
                                           ShowInHome = permission.ShowInHome ? true : false
                                       };

            return menusWithPermissions.ToList();
        }
    }
}

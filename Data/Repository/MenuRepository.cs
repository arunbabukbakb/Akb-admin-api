using Models;
using Data.Repository.IRepository;

namespace Data.Repository
{
    public class MenuRepository : Repository<Menu>, IMenuRepository
    {
        private ApplicationDbContext _db;
        public MenuRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<Menu> GetRoleMenu(int RoleCode)
        {
            //var menusWithPermissions = _db.Menu
            //.Where(menu => menu.MenuPermission != null && (menu.MenuPermission.Any(permission => permission.RoleCode == RoleCode) || RoleCode == 1) && menu.IsParent == true)
            //.Include(m => m.ChildMenus)
            //.ToList();

            var menusWithPermissions = _db.Menus
            .Where(menu => menu.MenuPermission != null &&
                           (menu.MenuPermission.Any(permission => permission.RoleId == RoleCode) || RoleCode == 1) &&
                           menu.IsParent == true && menu.Status == true)
            .Select(menu => new Menu
            {
                Icon = menu.Icon,
                Id = menu.Id,
                IsParent = true,
                MenuPermission = menu.MenuPermission,
                OrderNumber = menu.OrderNumber,
                ParentMenu = menu,
                Path = menu.Path,
                Title = menu.Title,
                Status = menu.Status,
                ChildMenus = menu.ChildMenus
                    .Where(childMenu => childMenu.MenuPermission != null && childMenu.Status == true &&
                                         (childMenu.MenuPermission.Any(permission => permission.RoleId == RoleCode) || RoleCode == 1))
                    .ToList()
            })
            .ToList();

            return menusWithPermissions;
        }

        public void Update(Menu obj)
        {
            _db.Menus.Update(obj);
        }
    }
}

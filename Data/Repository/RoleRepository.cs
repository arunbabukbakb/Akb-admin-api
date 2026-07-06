using Models;
using Data.Repository.IRepository;
namespace Data.Repository
{
	internal class RoleRepository : Repository<Role>, IRoleRepository
	{
		private ApplicationDbContext _db;
		public RoleRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}
		public void Update(Role obj)
		{
			var objFromdb = _db.Roles.FirstOrDefault(a => a.Id == obj.Id);
			if (objFromdb != null)
			{
	
			}
		}
	}
}

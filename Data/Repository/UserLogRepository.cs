using Models;
using Data.Repository.IRepository;

namespace Data.Repository
{
	public class UserLogRepository : Repository<UserLog>, IUserLogRepository
	{
		private ApplicationDbContext _db;
		public UserLogRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}
		public void Update(UserLog obj)
		{
			_db.UserLog.Update(obj);
		}		
	}
}

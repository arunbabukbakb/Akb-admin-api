using Models;
using Data.Repository.IRepository;
namespace Data.Repository
{
    public class UserBranchRepository : Repository<UserBranch>, IUserBranchRepository
	{
        private ApplicationDbContext _db;
        public UserBranchRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(UserBranch obj)
        {
			_db.UserBranches.Update(obj);
		}
    }
}

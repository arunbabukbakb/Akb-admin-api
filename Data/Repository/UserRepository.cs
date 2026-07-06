using Models;
using Data.Repository.IRepository;

namespace Data.Repository
{
    public class UserRepository : Repository<Users>, IUserRepository
    {
        private ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Users obj)
        {
			_db.Users.Update(obj);
		}
    }
}

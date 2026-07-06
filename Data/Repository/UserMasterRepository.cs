using Models;
using Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository
{
    

    public class UserMasterRepository : Repository<Users>, IUserMasterRepository
    {
        private ApplicationDbContext _db;
        public UserMasterRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Users obj)
        {
            _db.Users.Update(obj);
        }
    }
}

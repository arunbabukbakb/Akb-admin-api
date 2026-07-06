
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
    public interface IUserBranchRepository : IRepository<UserBranch>
    {
        void Update(UserBranch obj);
    }
}

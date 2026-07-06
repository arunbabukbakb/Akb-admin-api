using Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
   

    public interface IUserMasterRepository : IRepository<Users>
    {
        void Update(Users obj);
    }
}

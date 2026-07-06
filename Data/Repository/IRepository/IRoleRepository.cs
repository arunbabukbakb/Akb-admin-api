using Models;
using Data.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
	public interface IRoleRepository : IRepository<Role>
	{
		void Update(Role obj);
	}
}

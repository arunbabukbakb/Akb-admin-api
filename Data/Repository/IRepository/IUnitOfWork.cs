using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
	public interface IUnitOfWork : IDisposable
	{
        //Company
        ICompanyRepository Company { get; }
        IBranchRepository Branch { get; }

        //Users
        IUserRepository User { get; }
        IRoleRepository Role { get; }
        IUserBranchRepository UserBranch { get; }
        IMenuRepository Menu { get; }
        IMenuPermissionRepository MenuPermission { get; }
        IUserLogRepository UserLog { get; }     

        //Customer
        ICustomerRepository Customer { get; }
		
		void Save();
		void BeginTransaction();
		void Commit();
		void Rollback();
	}
}

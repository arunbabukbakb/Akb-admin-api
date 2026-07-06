using Data.Repository.IRepository;
using Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private ApplicationDbContext _db;
		private IDbContextTransaction? _transaction;
		public UnitOfWork(ApplicationDbContext db)
		{
			_db = db;
            //Company
            Company = new CompanyRepository(_db);
            Branch = new BranchRepository(_db);

            //User
            User = new UserRepository(_db);
            Role = new RoleRepository(_db);
            UserBranch = new UserBranchRepository(_db);
            Menu = new MenuRepository(_db);
            MenuPermission = new MenuPermissionRepository(_db);
            UserLog = new UserLogRepository(_db);

            //Customer			
            Customer = new CustomerRepository(_db);
        }

        //Company
        public ICompanyRepository Company { get; private set; }
        public IBranchRepository Branch { get; set; }

        //Users
        public IUserRepository User { get; private set; }
        public IRoleRepository Role { get; private set; }
        public IUserBranchRepository UserBranch { get; set; }
        public IMenuRepository Menu { get; set; }
        public IMenuPermissionRepository MenuPermission { get; set; }
        public IUserLogRepository UserLog { get; set; }

        //Customer
        public ICustomerRepository Customer { get; set; }

        public void Save()
		{
			_db.SaveChanges();
		}
		public void BeginTransaction()
		{
			_transaction = _db.Database.BeginTransaction();
		}

		public void Commit()
		{
			_transaction?.Commit();
		}

		public void Rollback()
		{
			_transaction?.Rollback();
		}

		// Implement IDisposable interface for proper cleanup if needed
		public void Dispose()
		{
			_transaction?.Dispose();
			_db.Dispose();
		}
	}
}

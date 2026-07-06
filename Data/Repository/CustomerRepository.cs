using Models;
using Models.ViewModels;
using Data.Repository.IRepository;

namespace Data.Repository
{
	public class CustomerRepository : Repository<Customer>, ICustomerRepository
	{
		private ApplicationDbContext _db;
		public CustomerRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}

		public string GenerateCustomerCode(int branchId)
		{
			var branch = _db.Branch.Where(a => a.Id == branchId).FirstOrDefault(); // Materialize the query to execute it in-memory

			int? maxNumber = _db.Customers.Where(c => c.BranchId == branchId)
				.Max(c => (int?)c.Number) ?? 0;

			//var maxNumber = _db.Customer.Select(o => (int?)o.Number)
			//	.DefaultIfEmpty(0)
			//	.Max(); // Calculate the maximum number in-memory

			var newNumber = maxNumber + 1;

			return $"{branch?.Code}{newNumber:D4}";
		}

		public Customer Auth2Login(Customer obj)
		{
			try
			{
				if (_db.Customers.Any(a => a.Email == obj.Email))
				{
					obj = _db.Customers.Where(c => c.Email == obj.Email).FirstOrDefault() ?? new Customer();
				}
				else
				{
					_db.Customers.Add(obj);
					_db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				var err = ex.Message;
			}
			return obj;
		}

		public bool CheckExistingCustomer(Customer obj)
		{
			bool returnvalue = false;
			try
			{
				if (_db.Customers.Any(a => a.Phone == obj.Phone || a.Email == obj.Email || a.UserName == obj.UserName))
				{
					returnvalue = true;
				}
			}
			catch (Exception ex)
			{
				var err = ex.Message;
			}
			return returnvalue;
		}

		public void Update(Customer obj)
		{
			_db.Customers.Update(obj);
		}

		public bool NotifyCustomer(int id, NotificationRequest request, INotificationService notificationService,string url)
		{
			bool returnvalue = true;
			if (id > 0)
			{
				var customer = _db.Customers.Where(u => u.Id == id).FirstOrDefault();
				if (customer != null)
				{
					request.Token = customer.FcmToken;
					notificationService.SendNotificationAsync(request,url);
				}
			}
			return returnvalue;
		}


	}
}

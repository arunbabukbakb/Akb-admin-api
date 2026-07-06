using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        public string GenerateCustomerCode(int branchId);

		void Update(Customer obj);
        Customer Auth2Login(Customer obj);

        bool CheckExistingCustomer(Customer obj);

		public bool NotifyCustomer(int id, NotificationRequest request, INotificationService notificationService,string url);
	}
}

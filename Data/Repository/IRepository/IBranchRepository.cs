using Models;
using Models;
using Models.ViewModels;
using Data.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
	public interface IBranchRepository : IRepository<Branch>
	{
		void Update(Branch obj);

		bool BranchApprove(int id);
		public bool NotifyBranchUsers(int branchId, NotificationRequest request, INotificationService notificationService, string url);
	}
}

using Models;
using Models.ViewModels;
using Data.Repository.IRepository;

namespace Data.Repository
{
	public class BranchRepository : Repository<Branch>, IBranchRepository
	{
		private ApplicationDbContext _db;
		public BranchRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}

		public bool BranchApprove(int id)
		{
			//Branch Approve
			var branch = _db.Branch.Where(x => x.Id == id).FirstOrDefault();
			if (branch != null)
			{
				branch.Status = !branch.Status;
				_db.Branch.Update(branch);
			}
			//var users = _db.Users.Where(u => u.UserBranch.BranchId == id).ToList();
			//if (users != null)
			//{
			//	foreach (var user in users)
			//	{
			//		user.Status = !user.Status;
			//		_db.Users.Update(user);
			//	}
			//}

			//if (branch == null && users == null)
			//{
			//	return false;
			//}
			//else
			//{
			//	return true;
			//}
			return true;
		}

		public bool NotifyBranchUsers(int branchId, NotificationRequest request, INotificationService notificationService, string url)
		{
			bool returnvalue = true;
			if (branchId > 0)
			{
				//var users = _db.Users.Where(u => u.UserBranch.BranchId == branchId).ToList();
				//if (users != null)
				//{
				//	foreach (var user in users)
				//	{
				//		request.Token = user.FcmToken;
				//		notificationService.SendNotificationAsync(request, url);
				//	}
				//}
			}
			return returnvalue;
		}

		public void Update(Branch obj)
		{
			_db.Branch.Update(obj);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
	public class UserBranchViewModel
	{
        public int UserId { get; set; }
        public List<int> BranchIds { get; set; } = new();
        public int DefaultBranchId { get; set; }
    }
}

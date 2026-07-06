using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
	public class MenuPermissionViewModel
	{
        public IEnumerable<MenuPermission>? MenuPermission { get; set; }
        public int RoleCode { get; set; }
    }
}

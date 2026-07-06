using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
	public class UserTokenRequest
	{
		public int UserId { get; set; }
		public string? Token { get; set; }
	}
}

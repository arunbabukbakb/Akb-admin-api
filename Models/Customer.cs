using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
	public class Customer
	{
		[Key]
		public int Id { get; set; }
		public string Code { get; set; } = "";
		public int Number { get; set; }
		[Required]
		public string Name { get; set; } = "";
		[Required]
		public string Address1 { get; set; } = "";
		public string Address2 { get; set; } = "";
		public string City { get; set; } = "";
		public int District { get; set; }
		public string DistrictName { get; set; } = "";
		public int State { get; set; }
		public string StateName { get; set; } = "";
		public int Pincode { get; set; }
		public string LandMark { get; set; } = "";
		[Required]
		public string Phone { get; set; } = "";
		public string Phone2 { get; set; } = "";
		public string Email { get; set; } = "";
		public string GstNumber { get; set; } = "";
		public string? Image { get; set; }
		public string UserName { get; set; } = "";
		public string Password { get; set; } = "";
		public bool IsAuth2 { get; set; } = false;
		public int BranchId { get; set; }
		public string FcmToken { get; set; } = "";

	}
}

using Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class MenuPermission
	{
		[Key]
        public int Id { get; set; }
        [Required]
		public int MenuId { get; set; }
        [ForeignKey("MenuId")]
        [ValidateNever]
        public Menu? Menu { get; set; }
		[NotMapped]
		[ValidateNever]
		public string MenuName { get; set; } = "";
        [Required]       
        public int RoleId { get; set; }
		[ValidateNever]
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }
		public bool CanView { get; set; }
		public bool CanAdd { get; set; }
		public bool CanEdit { get; set; }
		public bool CanDelete { get; set; }
		public bool ShowInHome { get; set; }
	}
}

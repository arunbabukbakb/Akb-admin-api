using Models.DtoModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class Menu
	{
        [Key]
        public int Id { get; set; }
		[Required]
		public string? Title { get; set; } = "";

        public string? Path { get; set; }
        public string? Icon { get; set; }
        public int OrderNumber { get; set; }
        public bool IsParent { get; set; }
        public bool Status { get; set; }
        // Additional properties...

        public int? ParentMenuId { get; set; } // Reference to the parent menu item
		[ValidateNever]
		[ForeignKey("ParentMenuId")]
		public Menu? ParentMenu { get; set; } // Navigation property to the parent menu item
		[JsonIgnore] // Ignore this property during serialization
		[ValidateNever]
		public List<Menu>? ChildMenus { get; set; } // Navigation property to the child menus
		[ValidateNever]
		public ICollection<MenuPermission>? MenuPermission { get; set; }
	}
}

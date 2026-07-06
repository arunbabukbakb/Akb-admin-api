using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DtoModels
{
	public class MenuDto
	{
		public int Id { get; set; }
		public string? Title { get; set; }
		public string? Path { get; set; }
		public string? Icon { get; set; }
		public int OrderNumber { get; set; }
		public bool IsParent { get; set; }
		public int? ParentMenuId { get; set; }
        public bool Status { get; set; }
	}
}

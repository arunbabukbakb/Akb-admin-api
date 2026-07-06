using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
	public class PaginatedViewModel<T>
	{
		public IEnumerable<T>? Data { get; set; }
		public int Page { get; set; }
		public int TotalCount { get; set; }
		public int PageSize { get; set; }
	}
}

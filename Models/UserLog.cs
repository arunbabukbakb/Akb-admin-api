using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class UserLog
	{
		[Key]
		public int LogId { get; set; }
		public int UserId { get; set; }
        public Users? User { get; set; }
        public string? LogLevel { get; set; }
		public string? LogMessage { get; set; }
		public DateTime Timestamp { get; set; }
    }
}

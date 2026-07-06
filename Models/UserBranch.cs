using Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Models
{
	public class UserBranch
	{
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(UserId))]
        public Users? User { get; set; }
        [Required]
        public int BranchId { get; set; }
        [ValidateNever]
        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ModifiedDate { get; set; }
		public int CreatedBy { get; set; }
		public int ModifiedBy { get; set; }
        [NotMapped]
        [ValidateNever]
        public bool Selected { get; set; }
        public bool IsDefault { get; set; }
    }
}

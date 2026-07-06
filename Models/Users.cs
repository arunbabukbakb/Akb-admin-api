using Models;
using Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }

        /* ===== BASIC LOGIN INFO ===== */
        [Required]
        public string? UserName { get; set; }

        [Required]
        public string? Password { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ValidateNever]
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        /* ===== PERSONAL DETAILS ===== */
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? NickName { get; set; }

        public string? Address { get; set; }

        /* ===== ORGANIZATION MAPPING ===== */
        public int CompanyId { get; set; }

        [ValidateNever]
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        [ValidateNever]
        public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();

        /* ===== PROFILE & MEDIA ===== */
        public string? Photo { get; set; }              // ProfilePhoto
        public string? Sign { get; set; }

        [NotMapped]
        [ValidateNever]
        public IFormFile? PhotoFile { get; set; }

        [NotMapped]
        [ValidateNever]
        public IFormFile? SignFile { get; set; }

        /* ===== SYSTEM STATUS ===== */
        public bool Status { get; set; } = true;        // Active / Inactive
        public bool IsDeleted { get; set; } = false;    // Soft delete

        public DateTime? LastLoginAt { get; set; }
        public string? FcmToken { get; set; }

        /* ===== AUDIT FIELDS ===== */
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

}

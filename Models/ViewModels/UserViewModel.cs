using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class UserViewModel
    {
        /* ===== BASIC LOGIN INFO ===== */
        public int Id { get; set; }
        public string? UserName { get; set; }

        // Password is included for creation/edit forms
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; } // Useful in UI validation

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public int RoleId { get; set; }
        public string? RoleName { get; set; }

        /* ===== PERSONAL DETAILS ===== */
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? NickName { get; set; }

        public string? Address { get; set; }
        public string? Notes { get; set; }

        /* ===== ORGANIZATION MAPPING ===== */
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime? JoinDate { get; set; }

        /* ===== SYSTEM STATUS ===== */
        public bool Status { get; set; } = true;

    }

}

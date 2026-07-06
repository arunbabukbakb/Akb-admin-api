using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Models.DtoModels
{
    public class UserDto
    {
        /* ===== BASIC LOGIN INFO ===== */
        public int Id { get; set; }
        public string? UserName { get; set; }
        // Usually, you should not expose Password in DTO
        // public string? Password { get; set; } 

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AlternatePhone { get; set; }

        public int RoleId { get; set; }
        public string? RoleName { get; set; }  // Optional: map Role.Name

        /* ===== PERSONAL DETAILS ===== */
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? NickName { get; set; }

        public string? Address { get; set; }
        public string? Notes { get; set; }

        /* ===== ORGANIZATION MAPPING ===== */
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; } // Optional: map Company.Name
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; } // Optional
        public int? DesignationId { get; set; }
        public string? DesignationName { get; set; } // Optional
        
        public string? EmployeeCode { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime? JoinDate { get; set; }

        /* ===== PROFILE & MEDIA ===== */
        public string? Photo { get; set; }
        public string? Sign { get; set; }

        /* ===== SYSTEM STATUS ===== */
        public bool Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? LastLoginAt { get; set; }

        /* ===== AUDIT FIELDS ===== */
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

}

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
    public class Branch
    {
        [Key]
        public int Id { get; set; }

        // Relationship
        [Required]
        public int CompanyId { get; set; }

        // Branch Identity
        [Required]
        public string Code { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        public string? ShortName { get; set; }

        // Contact
        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Phone]
        public string? AlternatePhone { get; set; }

        // Address
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public int? PinCode { get; set; }

        // Management
        public string? ManagerName { get; set; }

        [EmailAddress]
        public string? ManagerEmail { get; set; }

        [Phone]
        public string? ManagerPhone { get; set; }

        // Lifecycle
        [Required]
        public DateTime OpeningDate { get; set; }

        public DateTime? ClosingDate { get; set; }

        // Status
        public bool Status { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Company Company { get; set; }
    }

}

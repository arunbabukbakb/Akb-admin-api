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
    public class Company
    {
        [Key]
        public int Id { get; set; }

        // Company Identity
        [Required]
        public string Code { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        public string? ShortName { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? GSTNumber { get; set; }

        // Contact Details
        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Website { get; set; }

        // Address
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        // Branding
        public string? Logo { get; set; }

        [NotMapped]
        [ValidateNever]
        public IFormFile? LogoFile { get; set; }

        public string? Prefix { get; set; }
        public string? Suffix { get; set; }
        public string? OpeningTime { get; set; }
        public string? ClosingTime { get; set; }

        // Status
        public bool Status { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

}

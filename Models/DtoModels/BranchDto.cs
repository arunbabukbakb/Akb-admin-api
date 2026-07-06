using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DtoModels
{
    public class BranchDto
    {
        public int Id { get; set; }

        /* ===== RELATIONSHIP ===== */
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; } // optional display field

        /* ===== BRANCH IDENTITY ===== */
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? ShortName { get; set; }

        /* ===== CONTACT ===== */
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AlternatePhone { get; set; }

        /* ===== ADDRESS ===== */
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public int? PinCode { get; set; }

        /* ===== MANAGEMENT ===== */
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public string? ManagerPhone { get; set; }

        /* ===== LIFECYCLE ===== */
        public DateTime OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }

        /* ===== STATUS ===== */
        public bool Status { get; set; }
    }

}

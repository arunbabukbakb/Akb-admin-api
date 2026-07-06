using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    [Keyless]
    public class LoginResponseModel
    {
        public int UserId { get; set; }
        public string? RoleName { get; set; }
        public string? Image { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}

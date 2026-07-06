using Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Models
{
    public class Tokens
    {
        public Users? User { get; set; }
        public Customer? Customer { get; set; }
        public string? Logo { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Models.ViewModels
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }

        public string? NickName { get; set; }
    }
}

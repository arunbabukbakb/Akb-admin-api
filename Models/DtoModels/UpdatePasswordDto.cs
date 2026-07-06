using System.ComponentModel.DataAnnotations;

namespace Models.DtoModels
{
    public class UpdatePasswordDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Password must be at least 3 characters long.")]
        public string NewPassword { get; set; } = null!;
    }
}

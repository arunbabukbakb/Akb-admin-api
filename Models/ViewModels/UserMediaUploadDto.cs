using Microsoft.AspNetCore.Http;

namespace Models.ViewModels
{
    public class UserMediaUploadDto
    {
        public int UserId { get; set; }
        public IFormFile? Photo { get; set; }
        public IFormFile? Sign { get; set; }
    }
}

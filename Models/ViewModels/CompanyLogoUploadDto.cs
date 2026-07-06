using Microsoft.AspNetCore.Http;

namespace Models.ViewModels
{
    public class CompanyLogoUploadDto
    {
        public int CompanyId { get; set; }
        public IFormFile LogoFile { get; set; } = null!;
    }
}

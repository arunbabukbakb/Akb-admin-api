using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class PhotoUploadDto
    {
        public List<IFormFile> PhotoFiles { get; set; }
        public List<string> Photos { get; set; } // this could be populated from DB before update
    }

}

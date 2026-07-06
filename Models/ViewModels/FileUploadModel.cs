using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class FileUploadModel
    {
        public IFormFile File { get; set; }
        public string StockCode { get; set; }
        public string Result { get; set; } = "";
        public string SystemMessage { get; set; } = "";
        public bool Status { get; set; } = true;
    }
}

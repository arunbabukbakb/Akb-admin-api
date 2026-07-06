using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class CustomerViewModel
    {
        public Customer? Customer { get; set; }
        public IFormFile? File { get; set; } = null;
    }
}

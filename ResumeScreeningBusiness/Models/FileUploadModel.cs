using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumeScreeningBusiness.Models
{
    public class FileUploadModel
    {
        public IFormFile? File { get; set; }
    }
}

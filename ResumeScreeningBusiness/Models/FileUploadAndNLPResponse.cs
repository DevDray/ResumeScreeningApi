using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumeScreeningBusiness.Models
{
    public class FileUploadAndNLPResponse
    {
        public string filePath { get; set; }
        public List<ResumeEntitiesResponse> listOfResumeEntitiesResponse { get; set; }
    }
}

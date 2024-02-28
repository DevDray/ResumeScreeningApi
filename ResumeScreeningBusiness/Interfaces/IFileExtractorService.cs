using ResumeScreeningBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumeScreeningBusiness.Interfaces
{
    public interface IFileExtractorService
    {
        Task<string> ExtractText(FileUploadModel model);
        Task<ResumeEntitiesResponse> ExtractTextAndGetResumeEntities(string document);
    }
}

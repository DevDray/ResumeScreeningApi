using ResumeScreeningBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumeScreeningBusiness.Interfaces
{
    public interface IResumeScannerService
    {
        Task<ResumeEntitiesResponse> GetResumeEntities(string document);
    }
}

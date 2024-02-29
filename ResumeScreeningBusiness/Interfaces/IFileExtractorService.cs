using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage.Blob;
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
        Task<CloudBlockBlob> DownloadResume(string fileName);
        Task<FileUploadAndNLPResponse> ExtractTextAndGetResumeEntities(FileUploadModel document);
    }
}

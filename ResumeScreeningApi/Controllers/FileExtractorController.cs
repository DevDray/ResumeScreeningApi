using Azure;
using Azure.Storage.Blobs;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ResumeScreeningBusiness.Interfaces;
using ResumeScreeningBusiness.Models;
using System.IO;


[Route("api/[controller]")]
[ApiController]
public class FileExtractorController : ControllerBase
{
    private readonly IFileExtractorService _service;
    private readonly string _connectionString;
    public FileExtractorController(IFileExtractorService service)
    {
        _service = service;
        //_connectionString = configuration.GetConnectionString("AzureBlobStorageConnectionString");
        _connectionString = "DefaultEndpointsProtocol=https;AccountName=myaistorageaccount125;AccountKey=DpUWPDpAilrNkGTux1cNJ7nu2XTKYueiPrA3g3SeYuypuA4YtYTfr/93m8QrMfH+55ibZj4QHqSm+AStaIVvkw==;EndpointSuffix=core.windows.net";
    }

    [HttpPost("ExtractTextAndGetResumeEntities")]
    public async Task<IActionResult> ExtractTextAndGetResumeEntities([FromForm] FileUploadModel model)
    {
        var result = await _service.ExtractTextAndGetResumeEntities(model);
        return Ok(result);
    }

    [HttpPost("download")]
    public async Task<IActionResult> DownloadResume([FromBody]string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest("File name is required");
        var blockBlob = await _service.DownloadResume(fileName);
        if (blockBlob == null)
            return StatusCode(500, $"Internal server error");
        Stream blobStream = blockBlob.OpenReadAsync().Result;
        return File(blobStream, blockBlob.Properties.ContentType, blockBlob.Name);
    }
}

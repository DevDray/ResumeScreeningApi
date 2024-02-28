using Microsoft.AspNetCore.Mvc;
using ResumeScreeningBusiness.Interfaces;
using ResumeScreeningBusiness.Models;


[Route("api/[controller]")]
[ApiController]
public class FileExtractorController : ControllerBase
{
    private readonly IFileExtractorService _service;

    public FileExtractorController(IFileExtractorService service)
    {
        _service = service;
    }

    [HttpPost("ExtractTextAndGetResumeEntities")]
    public async Task<List<ResumeEntitiesResponse>> ExtractTextAndGetResumeEntities([FromForm] FileUploadModel model)
    {
        return await _service.ExtractTextAndGetResumeEntities(model);
    }
}

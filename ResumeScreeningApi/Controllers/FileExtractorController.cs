using DocumentFormat.OpenXml.EMMA;
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

    [HttpPost("extractText")]
    public async Task<IActionResult> ExtractText([FromForm] FileUploadModel model)
    {
        string result = string.Empty;
        try
        {
            if (model.File.Length > 0)
            {
                result = await _service.ExtractText(model);
            }
            else
            {
                return BadRequest("No file uploaded.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
        return Ok(result);
    }

    [HttpPost("ExtractTextAndGetResumeEntities")]
    public async Task<ResumeEntitiesResponse> ExtractTextAndGetResumeEntities([FromForm] FileUploadModel model)
    {
        var document = await _service.ExtractText(model);
        return await _service.ExtractTextAndGetResumeEntities(document);
    }
}

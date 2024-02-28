using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResumeScreeningBusiness.Interfaces;
using ResumeScreeningBusiness.Models;

namespace ResumeScreeningApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResumeScannerController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IResumeScannerService _service;
        public ResumeScannerController(ILogger<WeatherForecastController> logger, IResumeScannerService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public async Task<ResumeEntitiesResponse> GetResumeEntities([FromBody]string document)
        {
            return await _service.GetResumeEntities(document);
        }
    }
}

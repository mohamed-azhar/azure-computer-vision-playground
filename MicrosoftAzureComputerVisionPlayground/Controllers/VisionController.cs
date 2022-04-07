using Microsoft.AspNetCore.Mvc;
using MicrosoftAzureComputerVisionPlayground.Services;

namespace MicrosoftAzureComputerVisionPlayground.Controllers
{
    [ApiController]
    [Route("vision")]
    public class VisionController : ControllerBase
    {
        private readonly IComputerVisionService _computerVisionService;

        public VisionController(IComputerVisionService computerVisionService)
        {
            _computerVisionService = computerVisionService;
        }

        [HttpGet("read-text")]
        public async Task<ActionResult> ReadText(string url)
        {
            ArgumentNullException.ThrowIfNull(url, nameof(url));

            var result = await _computerVisionService.ReadAsync(url);

            return Ok(result);
        }
    }
}

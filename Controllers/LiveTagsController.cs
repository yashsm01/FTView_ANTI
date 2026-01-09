using Microsoft.AspNetCore.Mvc;
using AlarmMonitor.Services;

namespace AlarmMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveTagsController : ControllerBase
    {
        private readonly IOpcService _opcService;

        public LiveTagsController(IOpcService opcService)
        {
            _opcService = opcService;
        }

        [HttpGet("{tagName}")]
        public async Task<ActionResult<object>> GetLiveValue(string tagName)
        {
            // The tagName might need to be encoded or handled if it contains special characters
            // For now, we pass it directly to the service which will try to parse it as NodeId or format it.
            var value = await _opcService.ReadTagValueAsync(tagName);
            return Ok(new { TagName = tagName, Value = value, Timestamp = DateTime.Now });
        }
    }
}

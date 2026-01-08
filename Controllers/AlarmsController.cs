using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarmMonitor.Data;
using AlarmMonitor.Models;

namespace AlarmMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlarmsController : ControllerBase
    {
        private readonly AlarmContext _context;

        public AlarmsController(AlarmContext context)
        {
            _context = context;
        }

        // GET: api/Alarms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlarmEvent>>> GetAlarmEvents()
        {
            return await _context.AlarmEvents.ToListAsync();
        }
    }
}

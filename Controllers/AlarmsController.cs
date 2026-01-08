using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarmMonitor.Data;
using AlarmMonitor.Models;
using System.Linq;

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
        // Optional 'since' parameter to fetch only new alarms after a specific timestamp
        // Pagination parameters: pageNumber and pageSize
        [HttpGet]
        public async Task<ActionResult<object>> GetAlarmEvents(
            [FromQuery] DateTime? since = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            // If 'since' is provided, return unpaginated results for real-time polling
            if (since.HasValue)
            {
                var newAlarms = await _context.AlarmEvents
                    .Where(a => a.EventTimeStamp.HasValue && a.EventTimeStamp.Value > since.Value)
                    .OrderBy(a => a.EventTimeStamp)
                    .ToListAsync();
                return Ok(newAlarms);
            }
            
            // Pagination logic
            var page = pageNumber ?? 1;
            var size = pageSize ?? 10;
            
            if (page < 1) page = 1;
            if (size < 1) size = 10;
            if (size > 10) size = 10; // Capped at 10 as requested
            
            var totalCount = await _context.AlarmEvents.CountAsync();
            var alarms = await _context.AlarmEvents
                .OrderByDescending(a => a.EventTimeStamp)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            
            return Ok(new PaginatedResponse<AlarmEvent>
            {
                Data = alarms,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            });
        }
    }
}

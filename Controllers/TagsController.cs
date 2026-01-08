using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarmMonitor.Data;
using AlarmMonitor.Models;

namespace AlarmMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly AlarmContext _context;

        public TagsController(AlarmContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TagMaster>>> GetTagMasters(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 10) pageSize = 10; // Capped at 10 as requested
            
            var totalCount = await _context.TagMasters.CountAsync();
            var tags = await _context.TagMasters
                .OrderByDescending(t => t.UpdatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PaginatedResponse<TagMaster>
            {
                Data = tags,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TagMaster>> GetTagMaster(int id)
        {
            var tagMaster = await _context.TagMasters.FindAsync(id);

            if (tagMaster == null)
            {
                return NotFound();
            }

            return tagMaster;
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<ActionResult<TagMaster>> PostTagMaster(TagMaster tagMaster)
        {
            tagMaster.CreatedAt = DateTime.UtcNow;
            tagMaster.UpdatedAt = DateTime.UtcNow;
            
            _context.TagMasters.Add(tagMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTagMaster), new { id = tagMaster.Id }, tagMaster);
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTagMaster(int id, TagMaster tagMaster)
        {
            if (id != tagMaster.Id)
            {
                return BadRequest();
            }

            tagMaster.UpdatedAt = DateTime.UtcNow;
            _context.Entry(tagMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagMasterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTagMaster(int id)
        {
            var tagMaster = await _context.TagMasters.FindAsync(id);
            if (tagMaster == null)
            {
                return NotFound();
            }

            _context.TagMasters.Remove(tagMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TagMasterExists(int id)
        {
            return _context.TagMasters.Any(e => e.Id == id);
        }

        // POST: api/Tags/sync-from-alarm
        // Auto-sync tags from alarm SourceName
        [HttpPost("sync-from-alarm")]
        public async Task<ActionResult> SyncTagFromAlarm([FromBody] string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return BadRequest("SourceName cannot be empty");
            }

            // Check if tag already exists
            var existingTag = await _context.TagMasters
                .FirstOrDefaultAsync(t => t.TagName == sourceName);

            if (existingTag != null)
            {
                return Ok(new { message = "Tag already exists", tag = existingTag });
            }

            // Create new tag
            var newTag = new TagMaster
            {
                TagName = sourceName,
                Description = $"Auto-created from alarm event",
                HighLimit = 0,
                LowLimit = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TagMasters.Add(newTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTagMaster), new { id = newTag.Id }, newTag);
        }
    }
}

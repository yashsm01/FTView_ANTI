using Microsoft.EntityFrameworkCore;
using AlarmMonitor.Models;

namespace AlarmMonitor.Data
{
    public class AlarmContext : DbContext
    {
        public AlarmContext(DbContextOptions<AlarmContext> options) : base(options)
        {
        }

        public DbSet<AlarmEvent> AlarmEvents { get; set; }
    }
}

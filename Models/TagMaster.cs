using System;
using System.ComponentModel.DataAnnotations;

namespace AlarmMonitor.Models
{
    public class TagMaster
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string TagName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public double HighLimit { get; set; }
        
        public double LowLimit { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

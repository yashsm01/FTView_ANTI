using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlarmMonitor.Models
{
    [Table("AlarmEvents")]
    public class AlarmEvent
    {
        [Key]
        public Guid EventID { get; set; }
        public int? EventType { get; set; }
        [StringLength(200)]
        public string? SourceName { get; set; }
        [StringLength(400)]
        public string? SourcePath { get; set; }
        public Guid? SourceID { get; set; }
        [StringLength(50)]
        public string? ServerName { get; set; }
        public long? TicksTimeStamp { get; set; }
        public DateTime? EventTimeStamp { get; set; }
        [StringLength(50)]
        public string? EventCategory { get; set; }
        public int? Severity { get; set; }
        public int? Priority { get; set; }
        [StringLength(512)]
        public string? Message { get; set; }
        [StringLength(50)]
        public string? ConditionName { get; set; }
        [StringLength(50)]
        public string? SubConditionName { get; set; }
        [StringLength(1000)]
        public string? AlarmClass { get; set; }
        public bool? Active { get; set; }
        public bool? Acked { get; set; }
        public bool? EffDisabled { get; set; }
        public bool? Disabled { get; set; }
        public bool? EffSuppressed { get; set; }
        public bool? Suppressed { get; set; }
        [StringLength(50)]
        public string? PersonID { get; set; }
        public int? ChangeMask { get; set; }
        public double? InputValue { get; set; }
        public double? LimitValue { get; set; }
        public int? Quality { get; set; }
        public Guid? EventAssociationID { get; set; }
        [StringLength(512)]
        public string? UserComment { get; set; }
        [StringLength(64)]
        public string? ComputerID { get; set; }
        [StringLength(128)]
        public string? Tag1Value { get; set; }
        [StringLength(128)]
        public string? Tag2Value { get; set; }
        [StringLength(128)]
        public string? Tag3Value { get; set; }
        [StringLength(128)]
        public string? Tag4Value { get; set; }
        public DateTime? AutoUnshelveTime { get; set; }
        [StringLength(254)]
        public string? GroupPath { get; set; }
        public Guid? MessageID { get; set; }
    }
}

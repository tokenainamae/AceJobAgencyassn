using System;

namespace AceJobAgency.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int? MemberId { get; set; }  

        public string Action { get; set; } = "";

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? IpAddress { get; set; }
    }
}

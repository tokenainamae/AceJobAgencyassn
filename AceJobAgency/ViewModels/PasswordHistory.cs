using System;

namespace AceJobAgency.Models
{
    public class PasswordHistory
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public string PasswordHash { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

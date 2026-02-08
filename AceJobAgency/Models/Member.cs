using System.ComponentModel.DataAnnotations;

namespace AceJobAgency.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required] // To enc ltr
        public string NRIC { get; set; } = string.Empty; 

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string? ResumeFileName { get; set; }

        [MaxLength(1000)]
        public string? WhoAmI { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }
        public DateTime? LastPasswordChangedAt { get; set; }

    }
}

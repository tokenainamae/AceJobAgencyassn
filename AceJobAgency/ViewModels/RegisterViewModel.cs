using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace AceJobAgency.ViewModels
{
    public class RegisterViewModel
    {
        [Display(Name = "Profile Picture")]
        public IFormFile? ProfileImage { get; set; }

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public string Gender { get; set; } = "";

        [Required]
        public string NRIC { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [StrongPassword]
        public string Password { get; set; } = "";

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]

        public string ConfirmPassword { get; set; } = "";

        [Required]
        public DateTime DateOfBirth { get; set; }

        public IFormFile? Resume { get; set; }

        public string? WhoAmI { get; set; }

        // Captcha token
        [Required]
        public string? RecaptchaToken { get; set; }
    }

    public class StrongPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password))
                return new ValidationResult("Password is required");

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$");

            if (!regex.IsMatch(password))
            {
                return new ValidationResult(
                    "Password must be at least 12 characters and include uppercase, lowercase, number and special character"
                );
            }

            return ValidationResult.Success;
        }
    }

}

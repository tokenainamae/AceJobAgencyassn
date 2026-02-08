using System.ComponentModel.DataAnnotations;

namespace AceJobAgency.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }

}

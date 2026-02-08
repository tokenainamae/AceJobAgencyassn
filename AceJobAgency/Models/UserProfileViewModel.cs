namespace AceJobAgency.ViewModels
{
    public class UserProfileViewModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Gender { get; set; } = "";
        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; } = "";
        public string NRIC { get; set; } = "";       
        public string MaskedNRIC { get; set; } = "";  

        public string WhoAmI { get; set; } = "";
        public string? ResumeFileName { get; set; }
    }
}

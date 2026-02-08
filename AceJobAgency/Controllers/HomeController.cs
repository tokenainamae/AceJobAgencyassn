using System.Diagnostics;
using AceJobAgency.Models;
using AceJobAgency.Helpers;
using AceJobAgency.ViewModels;
using AceJobAgency.Data;
using Microsoft.AspNetCore.Mvc;

namespace AceJobAgency.Controllers
{
    [ServiceFilter(typeof(SessionAuthFilter))]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public HomeController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = _db.Members.FirstOrDefault(m => m.Id == userId);

            if (member == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var encryptionKey = _config["Encryption:Key"];
            var decryptedNric = CryptoHelper.Decrypt(member.NRIC, encryptionKey);

            var vm = new UserProfileViewModel
            {
                FirstName = member.FirstName,
                LastName = member.LastName,
                Gender = member.Gender,
                DateOfBirth = member.DateOfBirth,

                Email = member.Email,
                NRIC = decryptedNric,
                MaskedNRIC = MaskNRIC(decryptedNric),

                WhoAmI = member.WhoAmI,
                ResumeFileName = member.ResumeFileName
            };

            return View(vm);
        }

        private string MaskNRIC(string nric)
        {
            if (string.IsNullOrEmpty(nric) || nric.Length < 4)
                return "****";

            return nric.Substring(0, 1) + "****" + nric[^1];
        }
    }

}

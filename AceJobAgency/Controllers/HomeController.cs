using AceJobAgency.Data;
using AceJobAgency.Helpers;
using AceJobAgency.Models;
using AceJobAgency.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            var signature = HttpContext.Session.GetString("SessionSignature");
            var member = _db.Members.FirstOrDefault(m => m.Id == userId);

            if (userId == null || signature == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var expectedSignature =
                $"{userId}:{HttpContext.Connection.RemoteIpAddress}";

            if (signature != expectedSignature)
            {
                HttpContext.Session.Clear();
                return StatusCode(404);
            }


            if (member == null)
            {
                HttpContext.Session.Clear();
                return StatusCode(404);
            }

            var encryptionKey = _config["Encryption:Key"];
            var decryptedNric = CryptoHelper.Decrypt(member.NRIC, encryptionKey);

            var vm = new UserProfileViewModel
            {
                ProfileImage = member.ProfileImage,
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

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login");

            return View(new ChangePasswordViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetInt32("UserId");
            var member = _db.Members.FirstOrDefault(m => m.Id == userId);

            if (member == null)
                return RedirectToAction("Login");

            var hasher = new PasswordHasher<Member>();

            // verify current password
            var verifyCurrent = hasher.VerifyHashedPassword(
                member,
                member.PasswordHash,
                model.CurrentPassword
            );

            if (verifyCurrent == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Current password is incorrect");
                return View(model);
            }

            // cant use new pw 
            var reuseCheck = hasher.VerifyHashedPassword(
                member,
                member.PasswordHash,
                model.NewPassword
            );

            if (reuseCheck == PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "New password must be different from your current password."
                );
                return View(model);
            }

            // min password age
            if (member.LastPasswordChangedAt != null &&
                DateTime.Now < member.LastPasswordChangedAt.Value.AddMinutes(5))
            {
                ModelState.AddModelError(
                    "",
                    "You must wait at least 5 minutes before changing your password again."
                );
                return View(model);
            }

            // update password
            member.PasswordHash = hasher.HashPassword(member, model.NewPassword);
            member.LastPasswordChangedAt = DateTime.Now;

            _db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


    }
}

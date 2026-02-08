using AceJobAgency.Data;
using AceJobAgency.Models;
using AceJobAgency.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using AceJobAgency.Helpers;

namespace AceJobAgency.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext db, IWebHostEnvironment env, IConfiguration config, ILogger<AccountController> logger)
        {
            _db = db;
            _env = env;
            _config = config;
            _logger = logger;

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // recaptcha
            var secret = _config["GoogleReCaptcha:SecretKey"];
            var isHuman = await RecaptchaHelper.Verify(model.RecaptchaToken, secret);

            if (!isHuman)
            {
                ModelState.AddModelError("", "reCAPTCHA verification failed. Please try again.");
                return View(model);
            }

            // 1 email per acc check
            if (_db.Members.Any(m => m.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            // Pw hash
            var hasher = new PasswordHasher<Member>();
            var member = new Member();

            member.PasswordHash = hasher.HashPassword(member, model.Password);

            // Encrypt NRIC (AES)
            var key = _config["Encryption:Key"];
            member.NRIC = CryptoHelper.Encrypt(model.NRIC, key);

            member.FirstName = model.FirstName;
            member.LastName = model.LastName;
            member.Gender = model.Gender;
            member.Email = model.Email;
            member.DateOfBirth = model.DateOfBirth;
            member.WhoAmI = model.WhoAmI;

            // upload resume
            if (model.Resume != null)
            {
                var ext = Path.GetExtension(model.Resume.FileName).ToLower();
                if (ext != ".pdf" && ext != ".docx")
                {
                    ModelState.AddModelError("Resume", "Only PDF or DOCX allowed");
                    return View(model);
                }

                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(_env.WebRootPath, "uploads", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await model.Resume.CopyToAsync(stream);

                member.ResumeFileName = fileName;
            }

            _db.Members.Add(member);
            await _db.SaveChangesAsync();

            //pw history
            _db.PasswordHistories.Add(new PasswordHistory
            {
                MemberId = member.Id,
                PasswordHash = member.PasswordHash
            });

            await _db.SaveChangesAsync();


            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    MemberId = userId,
                    Action = "Logout",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                _db.SaveChanges();
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var member = _db.Members.FirstOrDefault(m => m.Email == model.Email);

            if (member == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            if (member.LockoutEnd != null && member.LockoutEnd > DateTime.Now)
            {
                ModelState.AddModelError("", "Account is locked. Please try again later.");
                return View(model);
            }

            var hasher = new PasswordHasher<Member>();
            var result = hasher.VerifyHashedPassword(member, member.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                member.FailedLoginAttempts++;

                if (member.FailedLoginAttempts >= 3)
                {
                    member.LockoutEnd = DateTime.Now.AddMinutes(10);
                }

                _db.AuditLogs.Add(new AuditLog
                {
                    MemberId = member.Id,
                    Action = "Failed Login",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                _db.SaveChanges();

                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // reset lockout counters
            member.FailedLoginAttempts = 0;
            member.LockoutEnd = null;
            _db.SaveChanges();

            // max pw age check
            if (member.LastPasswordChangedAt != null &&
                member.LastPasswordChangedAt.Value.AddDays(90) < DateTime.Now)
            {
                // only to change pw
                HttpContext.Session.SetInt32("UserId", member.Id);
                return RedirectToAction("ChangePassword", "Account");
            }

            // normal login
            HttpContext.Session.SetInt32("UserId", member.Id);
            HttpContext.Session.SetString("UserEmail", member.Email);

            _db.AuditLogs.Add(new AuditLog
            {
                MemberId = member.Id,
                Action = "Login",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            _db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
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

            // verify current password first
            var verify = hasher.VerifyHashedPassword(
                member,
                member.PasswordHash,
                model.CurrentPassword
            );

            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Current password is incorrect");
                return View(model);
            }

            // min pw age
            if (member.LastPasswordChangedAt != null &&
                DateTime.Now < member.LastPasswordChangedAt.Value.AddMinutes(5))
            {
                ModelState.AddModelError("",
                    "You must wait at least 5 minutes before changing your password again.");
                return View(model);
            }

            // change pw
            member.PasswordHash = hasher.HashPassword(member, model.NewPassword);
            member.LastPasswordChangedAt = DateTime.Now;

            _db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var member = _db.Members.FirstOrDefault(m => m.Email == model.Email);

            if (member != null)
            {
                member.PasswordResetToken = Guid.NewGuid().ToString();
                member.PasswordResetExpiry = DateTime.Now.AddMinutes(15);

                _db.SaveChanges();

                var resetLink = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = member.PasswordResetToken },
                    Request.Scheme
                );

                _logger.LogInformation("Password reset link: {ResetLink}", resetLink);
            }

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var member = _db.Members.FirstOrDefault(m =>
                m.PasswordResetToken == token &&
                m.PasswordResetExpiry > DateTime.Now);

            if (member == null)
            {
                return View("ResetPasswordInvalid"); // optional
            }

            return View(new ResetPasswordViewModel
            {
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var member = _db.Members.FirstOrDefault(m =>
                m.PasswordResetToken == model.Token &&
                m.PasswordResetExpiry > DateTime.Now);

            if (member == null)
            {
                ModelState.AddModelError("", "Invalid or expired reset link.");
                return View(model);
            }

            var hasher = new PasswordHasher<Member>();
            member.PasswordHash = hasher.HashPassword(member, model.NewPassword);
            member.LastPasswordChangedAt = DateTime.Now;

            member.PasswordResetToken = null;
            member.PasswordResetExpiry = null;

            _db.SaveChanges();

            return RedirectToAction("Login");
        }




    }


}

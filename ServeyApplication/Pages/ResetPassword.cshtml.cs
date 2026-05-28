using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using ServeyApplication.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace ServeyApplication.Pages.Account
{
    [EnableRateLimiting("ResetPolicy")]

    public class ResetPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResetPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        [BindProperty]
        [Required]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required]
        public string ConfirmPassword { get; set; }

        public bool TokenValid { get; set; }

        public IActionResult OnGet()
        {
            var reset = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == Token);

            TokenValid = reset != null &&
                         !reset.Used &&
                         reset.ExpiresAt > DateTime.UtcNow;

            return Page();
        }

        public IActionResult OnPost()
        {
            var reset = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == Token);

            if (reset == null || reset.Used || reset.ExpiresAt < DateTime.UtcNow)
            {
                TokenValid = false;
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                TokenValid = true;
                return Page();
            }

            if (NewPassword.Length < 6)
            {
                ModelState.AddModelError("", "Password must be at least 6 characters");
                TokenValid = true;
                return Page();
            }

            var user = _context.Users.Find(reset.UserId);
            if (user == null)
            {
                TokenValid = false;
                return Page();
            }

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(NewPassword));
            user.PasswordHash = Convert.ToBase64String(hash);

            reset.Used = true;
            _context.SaveChanges();

            TempData["Success"] = "Your password has been reset successfully.";
            return RedirectToPage("/Login");
        }
    }
}
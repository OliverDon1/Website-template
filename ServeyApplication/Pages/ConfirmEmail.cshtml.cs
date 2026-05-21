using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServeyApplication.Data;
using ServeyApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace ServeyApplication.Pages
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public string Message { get; set; }

        public ConfirmEmailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Message = "Invalid confirmation link.";
                return Page();
            }

            // Find the token + include the user
            var record = _context.EmailConfirmationTokens
                .Include(t => t.User)
                .FirstOrDefault(t => t.TokenString == token);

            if (record == null)
            {
                Message = "Invalid or expired confirmation token.";
                return Page();
            }

            // Check expiration (10 minutes)
            if (record.CreatedAt.AddMinutes(10) < DateTime.UtcNow)
            {
                Message = "This confirmation link has expired. Please request a new one.";
                return Page();
            }

            // Check if already confirmed
            if (record.User.EmailConfirmed)
            {
                Message = "Your email is already confirmed.";
                return Page();
            }

            // Mark user as confirmed
            record.User.EmailConfirmed = true;

            // Mark token as used (optional but recommended)
            record.ConfirmedAt = DateTime.UtcNow;

            _context.SaveChanges();

            Message = "Your email has been confirmed. You can now log in.";
            return Page();
        }
    }
}
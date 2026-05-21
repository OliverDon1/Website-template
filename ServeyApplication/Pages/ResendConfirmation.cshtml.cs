using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServeyApplication.Data;
using ServeyApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace ServeyApplication.Pages
{
    public class ResendConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public ResendConfirmationModel(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [BindProperty]
        public string Email { get; set; }

        public string Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Please enter your email.";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == Email);

            if (user == null)
            {
                Message = "No account found with that email.";
                return Page();
            }

            if (user.EmailConfirmed)
            {
                Message = "Your email is already confirmed.";
                return Page();
            }

            // Generate a new token
            var token = Guid.NewGuid().ToString("N");

            var emailToken = new EmailConfirmationToken
            {
                UserId = user.Id,
                TokenString = token,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailConfirmationTokens.Add(emailToken);
            _context.SaveChanges();

            // Build confirmation URL
            var confirmUrl = Url.Page(
                "/ConfirmEmail",
                null,
                new { token = token },
                Request.Scheme
            );

            // Send email
            await _emailService.SendEmailAsync(
                Email,
                "Confirm your email",
                $"<p>Click the link to confirm your email:</p><a href='{confirmUrl}'>Confirm Email</a>"
            );

            Message = "A new confirmation email has been sent.";
            return Page();
        }
    }
}
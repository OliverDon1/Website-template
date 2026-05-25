using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServeyApplication.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace ServeyApplication.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public ForgotPasswordModel(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = _context.Users.FirstOrDefault(u => u.Email == Email);

            // Always show the same message (security best practice)
            TempData["Message"] = "If an account with that email exists, a reset link has been sent.";

            if (user == null)
                return RedirectToPage();

            // Generate secure token
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes);

            var reset = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _context.PasswordResetTokens.Add(reset);
            _context.SaveChanges();

            var resetLink = Url.Page(
                "/ResetPassword",
                pageHandler: null,
                values: new { token = token },
                protocol: Request.Scheme
            );

            var subject = "Password Reset Request";

            var body = $@"
                <p>Hello,</p>
                <p>You requested a password reset. Click the link below to reset your password:</p>
                <p><a href=""{resetLink}"">Reset Password</a></p>
                <p>If you did not request this, you can safely ignore this email.</p>
                <p>Thanks,<br/>Your App Team</p>
            ";

            await _emailService.SendEmailAsync(user.Email, subject, body);

            return RedirectToPage();
        }
    }
}
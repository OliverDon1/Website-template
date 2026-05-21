using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServeyApplication.Data;
using ServeyApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace ServeyApplication.Pages
{
    public class CreateAccountModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public CreateAccountModel(ApplicationDbContext context, EmailService emailService   )
        {
            _context = context;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AccountValidityChecks();

            // NOW check ModelState
            if (!ModelState.IsValid)
                return Page();

            // Create user
            var user = new Models.User
            {
                Email = Input.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var token = Guid.NewGuid().ToString("N");
            var emailToken = new EmailConfirmationToken
            {
                UserId = user.Id,
                TokenString = token
            };

            _context.EmailConfirmationTokens.Add(emailToken);
            _context.SaveChanges();

            var confirmUrl = Url.Page(
            "/ConfirmEmail",
            pageHandler: null,
            values: new { token = token },
            protocol: Request.Scheme
            );

            await _emailService.SendEmailAsync(
            Input.Email,
            "Confirm your email",
            $"<p>Click the link to confirm your email:</p><a href='{confirmUrl}'>Confirm Email</a>"
            );

            return RedirectToPage("/Login");
        }

        public void AccountValidityChecks()
        {
            if (string.IsNullOrWhiteSpace(Input.Password) || Input.Password.Length < 12)
            {
                ModelState.AddModelError(string.Empty, "Password must be at least 12 characters long.");
            }

            if (Input.Password != Input.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
            }

            if (_context.Users.Any(u => u.Email == Input.Email))
            {
                ModelState.AddModelError(string.Empty, "An account with this email already exists.");
            }

            if (!Input.Password.Any(char.IsUpper))
            {
                ModelState.AddModelError(string.Empty, "Password must contain atleast one uppercase letter.");
            }

            if (!Input.Password.Any(char.IsDigit))
            {
                ModelState.AddModelError(string.Empty, "Password must contain at least one number.");
            }

            if (!Input.Password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                ModelState.AddModelError(string.Empty, "Password must contain at least one special character.");
            }

        }
    }
}
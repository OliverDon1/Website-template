using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServeyApplication.Data;
using ServeyApplication.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace ServeyApplication.Pages
{
    [EnableRateLimiting("LoginPolicy")]
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        // ⭐ Add RememberMe binding
        [BindProperty]
        public bool RememberMe { get; set; }

        public string Message { get; set; }

        public class InputModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // 1. Find user
            var user = _context.Users.FirstOrDefault(u => u.Email == Input.Username);

            if (user == null)
            {
                Message = "Invalid username or password.";
                return Page();
            }

            // 2. Check email confirmed
            if (!user.EmailConfirmed)
            {
                Message = "Your email is not confirmed. <a href=\"/ResendConfirmation\">Resend confirmation email</a>";
                return Page();
            }

            // 3. Check password
            bool validPassword = BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash);

            if (!validPassword)
            {
                Message = "Invalid username or password.";
                return Page();
            }

            // 4. Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            // ⭐ 5. Sign in with Remember Me support
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = RememberMe,
                ExpiresUtc = RememberMe ? DateTime.UtcNow.AddDays(14) : null
            };

            await HttpContext.SignInAsync("MyCookieAuth", principal, authProperties);

            return RedirectToPage("/Index");
        }
    }
}
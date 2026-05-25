using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServeyApplication.Data;
using ServeyApplication.Models;
using System.Security.Cryptography;
using System.Text;

namespace ServeyApplication.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> Users { get; set; }
        public User SelectedUser { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        public void OnGet()
        {
            Users = string.IsNullOrWhiteSpace(Search)
                ? _context.Users.ToList()
                : _context.Users
                    .Where(u => u.Email.Contains(Search))
                    .ToList();
        }

        public IActionResult OnPostSelectUser(int id)
        {
            LoadUsers();
            SelectedUser = _context.Users.Find(id);
            return Page();
        }

        public IActionResult OnPostResetPassword(int id)
        {
            LoadUsers();
            SelectedUser = _context.Users.Find(id);

            if (SelectedUser == null)
                return NotFound();

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
            {
                ModelState.AddModelError("", "Password must be at least 6 characters");
                return Page();
            }

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(NewPassword));
            SelectedUser.PasswordHash = Convert.ToBase64String(hash);

            _context.SaveChanges();

            TempData["Success"] = "Password reset successfully";
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToPage();
        }

        private void LoadUsers()
        {
            Users = string.IsNullOrWhiteSpace(Search)
                ? _context.Users.ToList()
                : _context.Users
                    .Where(u => u.Email.Contains(Search))
                    .ToList();
        }

        public IActionResult OnPostCleanTokens()
        {
            var expiredTokens = _context.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.Used)
                .ToList();

            if (expiredTokens.Any())
            {
                _context.PasswordResetTokens.RemoveRange(expiredTokens);
                _context.SaveChanges();
                TempData["CleanupMessage"] = $"Removed {expiredTokens.Count} expired tokens.";
            }
            else
            {
                TempData["CleanupMessage"] = "No expired tokens found.";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostPromote(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            user.Role = "Admin";
            _context.SaveChanges();

            TempData["Success"] = $"{user.Email} is now an Admin.";
            return RedirectToPage();
        }

        public IActionResult OnPostDemote(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            // Prevent self-demotion
            if (user.Email == User.Identity.Name)
            {
                TempData["Error"] = "You cannot remove your own admin rights.";
                return RedirectToPage();
            }

            // Prevent removing the last admin
            var adminCount = _context.Users.Count(u => u.Role == "Admin");
            if (adminCount <= 1)
            {
                TempData["Error"] = "You cannot remove the last remaining admin.";
                return RedirectToPage();
            }

            user.Role = "User";
            _context.SaveChanges();

            TempData["Success"] = $"{user.Email} is no longer an Admin.";
            return RedirectToPage();
        }
    }
}
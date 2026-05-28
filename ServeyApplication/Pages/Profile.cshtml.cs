using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServeyApplication.Data;
using ServeyApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace ServeyApplication.Pages.Account
{
    [Authorize]
    public partial class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryption;

        public ProfileModel(ApplicationDbContext context, IEncryptionService encryption)
        {
            _context = context;
            _encryption = encryption;
        }

        [BindProperty]
        public string DisplayName { get; set; }

        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public string Phone { get; set; }

        public void OnGet()
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                TempData["Error"] = "User account not found.";
                return;
            }

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id)
                          ?? CreateProfile(user);

            DisplayName = profile.DisplayName ?? "";
            FirstName = profile.FirstName ?? "";
            LastName = profile.LastName ?? "";

            Phone = string.IsNullOrEmpty(profile.Phone)
                ? ""
                : _encryption.Decrypt(profile.Phone) ?? "";
        }

        public IActionResult OnPost()
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                TempData["Error"] = "User account not found.";
                return RedirectToPage("/Login");
            }

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id)
                          ?? CreateProfile(user);

            profile.DisplayName = DisplayName ?? "";
            profile.FirstName = FirstName ?? "";
            profile.LastName = LastName ?? "";

            // Encrypt phone safely
            profile.Phone = string.IsNullOrWhiteSpace(Phone)
                ? null
                : _encryption.Encrypt(Phone);

            _context.SaveChanges();

            TempData["Success"] = "Profile updated.";
            return RedirectToPage();
        }

        private UserProfile CreateProfile(User user)
        {
            var profile = new UserProfile
            {
                UserId = user.Id,
                DisplayName = user.Email.Split('@')[0]
            };

            _context.UserProfiles.Add(profile);
            _context.SaveChanges();
            return profile;
        }
    }
}
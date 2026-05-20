using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ServeyApplication.Pages
{
    public class CreateAccountModel : PageModel
    {
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
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // TODO: Save user to database or identity system

            return RedirectToPage("/Login");
        }
    }
}
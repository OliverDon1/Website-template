using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServeyApplication.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // TODO: Add authentication logic here

            return RedirectToPage("/Index");
        }
    }

}

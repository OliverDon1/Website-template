using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace ServeyApplication.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            var isAuth = User.Identity.IsAuthenticated;
            Console.WriteLine("Authenticate " + isAuth);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace TicketSupportSystem.Pages.UserView
{
    [EnableRateLimiting("auth")]
    public class ForgotPasswordModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}

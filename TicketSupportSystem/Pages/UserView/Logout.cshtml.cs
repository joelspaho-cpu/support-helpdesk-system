using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.Services;
using TicketSupportSystem.ViewModels;

namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("/UserView/Login");
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await this.SignOutUserAsync();
            TempData["SuccessMessage"] = "You have been signed out successfully";
            return RedirectToPage("/UserView/Login");
        }
    }
}
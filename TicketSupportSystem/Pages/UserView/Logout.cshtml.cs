using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.Services;
using TicketSupportSystem.ViewModels;

namespace TicketSupportSystem.Pages.UserView
{
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
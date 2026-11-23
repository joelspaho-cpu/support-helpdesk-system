using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSupportSystem.Models;

namespace TicketSupportSystem.Pages.StaffView
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<Staff> _signInManager;
        public LogoutModel(SignInManager<Staff> signInManager)
        {
            _signInManager = signInManager;
        }
        public IActionResult OnGet()
        {
            return RedirectToPage("/StaffView/Login");
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been signed out successfully";
            return RedirectToPage("/StaffView/Login");
        }
    }
}
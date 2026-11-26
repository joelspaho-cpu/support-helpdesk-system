using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using TicketSupportSystem.Models;

namespace TicketSupportSystem.Pages.StaffView
{
    [EnableRateLimiting("auth")]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Staff> _signInManager;
        public LoginModel(SignInManager<Staff> signInManager)
        {
            _signInManager = signInManager;
        }
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "This field cannot be empty"), EmailAddress]
        public string Email {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "This field cannot be empty"), DataType(DataType.Password)]
        public string Password {get; set;} = string.Empty;
        public IActionResult OnGet()
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToPage("/StaffView/Dashboard");
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            if (_signInManager.IsSignedIn(User)) return RedirectToPage("/StaffView/Dashboard");
            var result = await _signInManager.PasswordSignInAsync(Email, Password, isPersistent: false, lockoutOnFailure: true);
            if (result.Succeeded) {
                if (Url.IsLocalUrl(ReturnUrl)) return LocalRedirect(ReturnUrl);
                return RedirectToPage("/StaffView/Dashboard");}
            else { ModelState.AddModelError(nameof(Email), "Unable to sign in. If this keeps happening, contact your manager."); }

            return Page();
        }

    }
}

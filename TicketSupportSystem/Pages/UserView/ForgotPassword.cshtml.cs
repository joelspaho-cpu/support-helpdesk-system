using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using TicketSupportSystem.Services;

namespace TicketSupportSystem.Pages.UserView
{
    [EnableRateLimiting("auth")]
    public class ForgotPasswordModel : PageModel
    {
        private readonly IVerificationService _verify;
        [BindProperty]
        [Required, EmailAddress, MaxLength(254)]
        public string Email {get; set;} = string.Empty;
        public ForgotPasswordModel(IVerificationService verify)
        {
            _verify = verify;
        }
        public IActionResult OnGet()
        {
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var result = await _verify.SendForgotPasswordLinkAsync(Email);
            if (!result) {
                TempData["ErrorMessage"] = "You are sending too many emails, try again later";
                return Page();
                }
            TempData["SuccessMessage"] = "If this email exists, we have sent a link that allows you to reset your password";
            return RedirectToPage("/UserView/Login");
        }
    }
}

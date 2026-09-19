using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSupportSystem.Services;

namespace TicketSupportSystem.Pages.UserView
{
    public class VerifyEmailModel : PageModel
    {
        private readonly IVerificationService _verify;
        [BindProperty]
        [Required, Range(100000, 999999, ErrorMessage = "Enter the 6-digit code")]
        public int EnteredCode {get; set;}
        public bool CanResend { get; set; } = true;
        public VerifyEmailModel(IVerificationService verify)
        {
            _verify = verify;
        }
        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            CanResend = await _verify.CanResendAsync(id);
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (!ModelState.IsValid) return Page();
            var verify = await _verify.VerifyAsync(id, EnteredCode);
            CanResend = await _verify.CanResendAsync(id); 
            switch (verify)
                {
                case VerificationResult.NotFound:
                    TempData["ErrorMessage"] = "An error has occurred, please attempt registration again.";
                    return RedirectToPage("/UserView/Register");
                case VerificationResult.Expired:
                    ModelState.AddModelError(nameof(EnteredCode), "The code you entered has expired, please attempt to resend a new one.");
                    return Page();
                case VerificationResult.TooManyAttempts:
                    ModelState.AddModelError(nameof(EnteredCode),"You have entered codes too many times, try again later.");
                    return Page();
                case VerificationResult.InvalidCode:
                    ModelState.AddModelError(nameof(EnteredCode), "The code you have entered is invalid");
                    return Page();
                case VerificationResult.Success:
                    TempData["SuccessMessage"] = "Your account has been created successfully.";
                    return RedirectToPage("/UserView/Login");
                }
            TempData["ErrorMessage"] = "An unknown error has occurred";
            return Page();
        }
        public async Task<IActionResult> OnPostResendAsync(Guid id)
        {
                var result = await _verify.ResendAsync(id);
                switch (result)
                {
                    case ResendResult.NotFound:
                        TempData["ErrorMessage"] = "An error has occurred, please attempt registration again.";
                        return RedirectToPage("/UserView/Register");
                    case ResendResult.TooManyResends:
                        TempData["ErrorMessage"] = "You have resent codes too many times, enter the last code you received or try again later.";
                        return RedirectToPage("/UserView/VerifyEmail", new { id });
                    case ResendResult.Success:
                        TempData["SuccessMessage"] = "We have sent you a code, check your inbox.";
                        return RedirectToPage("/UserView/VerifyEmail", new { id });
                }
                TempData["ErrorMessage"] = "An unknown error has occurred";
                return Page();
}
    }
}

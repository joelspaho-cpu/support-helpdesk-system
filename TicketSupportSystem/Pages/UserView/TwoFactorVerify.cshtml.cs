using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using TicketSupportSystem.Services;

namespace TicketSupportSystem.Pages.UserView
{
    [EnableRateLimiting("auth")]
    public class TwoFactorVerifyModel : PageModel
    {
        private readonly IVerificationService _verify;
        private readonly IUserService _user;
        public bool CanResend { get; set; } = true;
        [BindProperty]
        [Required]
        public int EnteredCode {get; set;}
        public TwoFactorVerifyModel(IVerificationService verify, IUserService user)
        {
            _verify = verify;
            _user = user;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        { 
            CanResend = await _verify.CanResendLoginAsync(id);
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (!ModelState.IsValid) return Page();
            var outcome = await _verify.TwoFactorVerifyAsync(id, EnteredCode);
            CanResend = await _verify.CanResendLoginAsync(id); 
            switch (outcome.Result)
            {
                case VerificationResult.Success:
                       var principal = _user.ConstructPrincipal(outcome.UserID!.Value);
                       await HttpContext.SignInAsync("UserScheme", principal, new AuthenticationProperties { IsPersistent = outcome.IsPersistent });
                       return RedirectToPage("/UserView/Dashboard");
                case VerificationResult.Expired:
                       ModelState.AddModelError(nameof(EnteredCode), "This code has expired, please request a new one");
                        return Page();
                case VerificationResult.InvalidCode:
                        ModelState.AddModelError(nameof(EnteredCode), "The code you have entered is invalid");
                        return Page();
                case VerificationResult.NotFound:
                        TempData["ErrorMessage"] = "An error has occurred, please try logging in again";
                        return RedirectToPage("/UserView/Login");
                case VerificationResult.TooManyAttempts:
                        ModelState.AddModelError(nameof(EnteredCode),"You have entered codes too many times, try again later");
                        return Page();
            }
            TempData["ErrorMessage"] = "An error has occurred, please try logging in again";
            return RedirectToPage("/UserView/Login");
        }
        public async Task<IActionResult> OnPostResendAsync(Guid id)
            {
                        var resend = await _verify.ResendLoginAsync(id);
                        switch (resend)
                            {
                                case ResendResult.NotFound:
                                    TempData["ErrorMessage"] = "An error has occurred, please try logging in again";
                                    return RedirectToPage("/UserView/Login");
                                case ResendResult.TooManyResends:
                                    TempData["ErrorMessage"] = "You have resent codes too many times, enter the last code you received or try again later";
                                    return RedirectToPage("/UserView/TwoFactorVerify", new { id });
                                case ResendResult.Success:
                                    TempData["SuccessMessage"] = "A new code has been sent, please check your inbox";
                                    return RedirectToPage("/UserView/TwoFactorVerify", new { id });
                            }
                TempData["ErrorMessage"] = "An error has occurred, please try logging in again";
                return RedirectToPage("/UserView/Login");
            }
    }
}

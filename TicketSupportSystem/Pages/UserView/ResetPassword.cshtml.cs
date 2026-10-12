using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using TicketSupportSystem.Services;

namespace TicketSupportSystem.Pages.UserView
{
    [EnableRateLimiting("auth")]
    public class ResetPasswordModel : PageModel
    {
        private readonly IVerificationService _verify;
        [BindProperty]
        [Required(ErrorMessage = "Please enter a password"), DataType(DataType.Password), MinLength(8, ErrorMessage ="The password must be at least 8 characters long"), MaxLength(100)]
        public string NewPassword {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please re-enter the password"), DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage ="Your passwords do not match"), MaxLength(100)]
        public string ConfirmNewPassword {get; set;} = string.Empty;
        public ResetPasswordModel(IVerificationService verify)
        {
            _verify = verify;
        }
        public async Task<IActionResult> OnGetAsync(string token)
        {
            var result = await _verify.ValidateForgotPasswordLinkAsync(token);
            if (!result) {
                TempData["ErrorMessage"] = "This link is not valid, try resetting your password again";
                return RedirectToPage("/UserView/ForgotPassword");
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string token)
        {
            if (!ModelState.IsValid) return Page();
            var result = await _verify.CompleteResetAsync(token, NewPassword);
            if (!result)
            {
                TempData["ErrorMessage"] = "Something went wrong while trying to reset your password, please try again";
                return Page();
            }
            TempData["SuccessMessage"] = "Your Password has been changed successfully";
            return RedirectToPage("/UserView/Login");
        }
    }
}
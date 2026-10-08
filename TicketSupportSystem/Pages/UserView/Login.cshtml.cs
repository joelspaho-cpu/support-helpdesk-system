using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TicketSupportSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;


namespace TicketSupportSystem.Pages.UserView
{
    [EnableRateLimiting("auth")]
    public class LoginModel : PageModel
    {
        private readonly IUserService _user;
        private readonly IVerificationService _verify;
        [BindProperty]
        [Required(ErrorMessage = "This field cannot be empty"), EmailAddress]
        public string Email {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "This field cannot be empty"), DataType(DataType.Password)]
        public string Password {get; set;} = string.Empty;
        [BindProperty]
        public bool RemainSignedIn {get; set;}
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/UserView/Dashboard");
            return Page();
        }
        public LoginModel (IUserService user, IVerificationService verify)
        {
            _user = user;
            _verify = verify;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/UserView/Dashboard"); 
            var outcome = await _user.AuthenticateAsync(Email, Password);
            switch (outcome.Result)
            {
                case LoginResult.InvalidCredentials:
                    ModelState.AddModelError(nameof(Email), "Invalid email or password");
                    return Page();
                case LoginResult.Locked:
                    ModelState.AddModelError(nameof(Email), "Your account is locked for security purposes, please reset your password to regain access");
                    return Page();
                case LoginResult.Success:
                    if (outcome.User!.Has2fa) {
                    var prospect = await _verify.StartLoginAsync(outcome.User.UserID, RemainSignedIn);
                    if (prospect == null) { ModelState.AddModelError(nameof(Email), "You cannot sign in at this time, try again later or register if you haven't already"); return Page(); }
                    return RedirectToPage("/UserView/TwoFactorVerify", new { id = prospect.Value });}
                    var principal = _user.ConstructPrincipal(outcome.User.UserID);
                    await HttpContext.SignInAsync("UserScheme", principal, new AuthenticationProperties { IsPersistent = RemainSignedIn });
                    return RedirectToPage("/UserView/Dashboard");
            }
            ModelState.AddModelError(nameof(Email), "Please attempt to login again");
            return Page();
            }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TicketSupportSystem.Services;
using Microsoft.AspNetCore.Authentication;


namespace TicketSupportSystem.Pages.UserView
{
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
            var user = await _user.AuthenticateAsync(Email, Password);
            if (user == null) {
                ModelState.AddModelError(nameof(Email), "Invalid email or password.");
                return Page(); }
            if (user.Has2fa) {
                var prospect = await _verify.StartLoginAsync(user.UserID, RemainSignedIn);
                if (prospect == null) { ModelState.AddModelError(nameof(Email), "You cannot sign in at this time, try again later or register if you haven't already"); return Page(); }
                return RedirectToPage("/UserView/TwoFactorVerify", new { id = prospect.Value });}
            var principal = _user.ConstructPrincipal(user.UserID);
            await HttpContext.SignInAsync("UserScheme", principal, new AuthenticationProperties { IsPersistent = RemainSignedIn });
            return RedirectToPage("/UserView/Dashboard");
            }
    }
}
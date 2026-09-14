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
        [BindProperty]
        [Required, EmailAddress]
        public string Email {get; set;} = string.Empty;
        [BindProperty]
        [Required, DataType(DataType.Password)]
        public string Password {get; set;} = string.Empty;
        [BindProperty]
        public bool RemainSignedIn {get; set;}
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/UserView/Dashboard");
            return Page();
        }
        public LoginModel (IUserService user)
        {
            _user = user;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await _user.AuthenticateAsync(Email, Password);
            if (user == null) return Page();
            var principal = _user.ConstructPrincipal(user);
            await HttpContext.SignInAsync("UserScheme", principal, new AuthenticationProperties { IsPersistent = RemainSignedIn });
            return RedirectToPage("/UserView/Dashboard");
            }
    }
}
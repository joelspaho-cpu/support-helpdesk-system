using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

using TicketSupportSystem.Services;


namespace TicketSupportSystem.Pages.UserView
{
    public class RegisterModel : PageModel
    {
        private readonly IVerificationService _verify;
        [BindProperty]
        [Required(ErrorMessage = "Please enter a valid display name"), MaxLength(50, ErrorMessage ="The display name may not exceed 50 characters")]
        public string DisplayName {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please enter a valid email"), EmailAddress(ErrorMessage = "This email is invalid"), MaxLength(254, ErrorMessage ="The email may not exceed 254 characters")]
        public string Email {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please enter a password"), DataType(DataType.Password), MinLength(8, ErrorMessage ="The password must be at least 8 characters long"), MaxLength(100)]
        public string Password {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please re-enter the password"), DataType(DataType.Password), Compare(nameof(Password), ErrorMessage ="Your passwords do not match"), MaxLength(100)]
        public string ConfirmPassword {get; set;} = string.Empty;
        [BindProperty]
        public bool Has2fa {get; set;}
        [BindProperty]
        [Required(ErrorMessage = "Please select your region from the dropdown list"), MaxLength(10)]
        public string Region {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please select your language from the dropdown list"), MaxLength(10)]
        public string Language {get; set;} = string.Empty;
        public RegisterModel(IVerificationService verify)
        {
             _verify = verify;
        }
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/UserView/Dashboard");
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid) return Page();
          var register = await _verify.StartAsync(DisplayName, Email, Password, Region, Language, Has2fa);
          if (register == null) { ModelState.AddModelError(nameof(Email), "This email cannot be used at this time, please try again later"); return Page(); }
          return RedirectToPage("/UserView/VerifyEmail", new { id = register.Value });
        }
    }
}

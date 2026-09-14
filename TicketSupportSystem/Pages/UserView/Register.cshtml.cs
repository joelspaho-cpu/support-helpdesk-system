using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

using TicketSupportSystem.Services;


namespace TicketSupportSystem.Pages.UserView
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _user;
        [BindProperty]
        [Required(ErrorMessage = "Please enter a valid display name"), MaxLength(50)]
        public string DisplayName {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please enter a valid email"), EmailAddress, MaxLength(254)]
        public string Email {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please enter a password"), DataType(DataType.Password), MinLength(8), MaxLength(100)]
        public string Password {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please re-enter the password"), DataType(DataType.Password), Compare(nameof(Password)), MinLength(8), MaxLength(100)]
        public string ConfirmPassword {get; set;} = string.Empty;
        [BindProperty]
        public bool Has2fa {get; set;}
        [BindProperty]
        [Required(ErrorMessage = "Please select your region from the dropdown list"), MaxLength(10)]
        public string Region {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please select your language from the dropdown list"), MaxLength(10)]
        public string Language {get; set;} = string.Empty;
        public RegisterModel(IUserService user)
        {
             _user = user;
        }
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/UserView/Dashboard");
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid) return Page();
          var register = await _user.RegisterAsync(DisplayName, Email, Password, Region, Language);
          if (register == null) return Page(); 
          return RedirectToPage("/UserView/Dashboard");
        }
    }
}

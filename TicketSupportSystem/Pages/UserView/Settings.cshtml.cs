using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.Services;


namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public bool Has2Fa {get; set;}
        [BindProperty]
        [Required(ErrorMessage = "Please enter a valid display name"), MaxLength(50, ErrorMessage ="The display name may not exceed 50 characters")]
        public string DisplayName {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Enter your current password to save changes"), DataType(DataType.Password), MaxLength(100)]
        public string CurrentPassword {get; set;} = string.Empty;
        [BindProperty]
        [DataType(DataType.Password), MinLength(8, ErrorMessage ="The password must be at least 8 characters long"), MaxLength(100)]
        public string? NewPassword {get; set;}
        [BindProperty]
        [DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage ="Your passwords do not match"), MaxLength(100)]
        public string? ConfirmNewPassword {get; set;}
        [BindProperty]
        [Required(ErrorMessage = "Please select your region from the dropdown list"), MaxLength(10)]
        public string Region {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please select your language from the dropdown list"), MaxLength(10)]
        public string Language {get; set;} = string.Empty;
        private readonly IUserService _user;
        public SettingsModel(IUserService user)
        {
            _user = user;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var userID = User.GetId();
            if (userID == null) 
            {
                TempData["ErrorMessage"] = "An error has occurred, please attempt to sign in again";
                await this.SignOutUserAsync();
                return RedirectToPage("/UserView/Login");
            }
            var result = await _user.GetSettingsAsync(userID.Value);
            if (result == null) 
            {
                TempData["ErrorMessage"] = "An error has occurred, please attempt to sign in again";
                await this.SignOutUserAsync();
                return RedirectToPage("/UserView/Login");
            }
            
            Has2Fa = result.Has2Fa;
            DisplayName = result.DisplayName;
            Email = result.Email;
            Region = result.Region;
            Language = result.Language;
            
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userID = User.GetId();
            if (userID == null) return RedirectToPage("/UserView/Login");

            var result = await _user.ChangeSettingsAsync(userID.Value, CurrentPassword, Has2Fa, DisplayName, Region, Language, NewPassword);
            switch (result)
            {
                case SettingsUpdateResult.NotFound:
                    TempData["ErrorMessage"] = "An error has occurred, please attempt to sign in again";
                    await this.SignOutUserAsync();
                    return RedirectToPage("/UserView/Login");
                case SettingsUpdateResult.Success:
                    TempData["SuccessMessage"] = "Your changes have been saved";
                    return RedirectToPage();
                case SettingsUpdateResult.WrongPassword:
                    ModelState.AddModelError(nameof(CurrentPassword), "Please enter your password correctly to save changes");
                    return Page();
                case SettingsUpdateResult.NoChanges:
                    TempData["ErrorMessage"] = "There are no changes to save";
                    return Page();
            }
            return Page();
        }
    }
}
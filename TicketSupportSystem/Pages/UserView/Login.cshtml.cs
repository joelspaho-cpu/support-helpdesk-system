using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TicketSupportSystem.Data;
using TicketSupportSystem.Services;
using TicketSupportSystem.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace TicketSupportSystem.Pages.UserView
{
    public class LoginModel : PageModel
    {
        private readonly IHashingService _hasher;
        private readonly AppDbContext _db;
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
        public LoginModel (IHashingService hasher, AppDbContext db)
        {
            _hasher = hasher;
            _db = db;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            Email = Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user == null) 
            {
                var dummyPass = _hasher.DummyHashVerify(Password); // making response times equal in both cases
                ModelState.AddModelError("Email", "The email or password is invalid");
                return Page();
            }
            var passResult = _hasher.Verify(Password, user.PasswordHash);
            switch (passResult)
            {
                case HashCheckResult.Failed:
                    ModelState.AddModelError("Email", "The email or password is invalid");
                    return Page();
                case HashCheckResult.SuccessRehashNeeded:
                    var newPass = _hasher.Hash(Password);
                    user.PasswordHash = newPass;
                    await _db.SaveChangesAsync();
                    break;
            }
            var claims = new List<Claim>
                {
                  new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.UserID))
                };
            var identity = new ClaimsIdentity(claims, "UserScheme");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("UserScheme", principal, new AuthenticationProperties { IsPersistent = RemainSignedIn });
            return RedirectToPage("/UserView/Dashboard");
            }
    }
}
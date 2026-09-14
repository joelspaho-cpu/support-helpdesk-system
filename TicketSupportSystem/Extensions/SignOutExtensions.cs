using Microsoft.AspNetCore.Mvc;       
using Microsoft.AspNetCore.Mvc.RazorPages;  
using Microsoft.AspNetCore.Authentication; 

namespace TicketSupportSystem.Extensions;
public static class SignOutExtensions{
public static async Task<IActionResult> SignOutUserAsync(this PageModel page)
{
    await page.HttpContext.SignOutAsync("UserScheme");
    return page.RedirectToPage("/UserView/Login");
}
}
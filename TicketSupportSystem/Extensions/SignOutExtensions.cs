using Microsoft.AspNetCore.Mvc;       
using Microsoft.AspNetCore.Mvc.RazorPages;  
using Microsoft.AspNetCore.Authentication;
using TicketSupportSystem.Models;

namespace TicketSupportSystem.Extensions;
public static class SignOutExtensions{
public static async Task<IActionResult> SignOutUserAsync(this PageModel page)
{
    await page.HttpContext.SignOutAsync(AuthSchemes.User);
    return page.RedirectToPage("/UserView/Login");
}
}
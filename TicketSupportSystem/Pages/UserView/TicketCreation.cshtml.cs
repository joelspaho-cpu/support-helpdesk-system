using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSupportSystem.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.Services;


namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class TicketCreationModel : PageModel
    {
        private readonly ITicketService _tk;
        [BindProperty]
        [Required(ErrorMessage = "Subject cannot be empty"), MaxLength(30)]
        public string Subject {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Description cannot be empty"), MaxLength(5000)]
        public string Description {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please select a query type")]
        public TicketQuery? Query {get; set;}
        public TicketCreationModel (ITicketService tk)
        {
            _tk = tk;
        }
        public IActionResult OnGet()
        {
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userID = User.GetId();
            if (userID == null) { return await this.SignOutUserAsync(); } 
            int ticketIDAfterCreation = await _tk.CreateTicketAsync(userID.Value, Subject, Description, Query!.Value, ip);
            return RedirectToPage("/UserView/TicketViewer", new { id = ticketIDAfterCreation });
        }
    }
}

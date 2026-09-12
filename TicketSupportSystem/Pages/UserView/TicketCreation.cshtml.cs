using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSupportSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using TicketSupportSystem.Data;
using TicketSupportSystem.Extensions;


namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class TicketCreationModel : PageModel
    {
        private readonly AppDbContext _db; 
        [BindProperty]
        [Required(ErrorMessage = "Subject cannot be empty"), MaxLength(30)]
        public string Subject {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Descripton cannot be empty"), MaxLength(5000)]
        public string Description {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please select a query type")]
        public TicketQuery? Query {get; set;}
        public TicketCreationModel (AppDbContext db)
        {
            _db = db;
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
            if (userID == null) { await HttpContext.SignOutAsync("UserScheme"); return RedirectToPage("/UserView/Login"); } 
            Ticket Ticket = new Ticket{
                UserID = userID.Value,
                Subject = Subject,
                Description = Description,
                Query = Query!.Value,
                IPAddress = ip,
                CreatedAt = DateTime.UtcNow
            };
            _db.Tickets.Add(Ticket);
            await _db.SaveChangesAsync();
            return RedirectToPage("/UserView/TicketViewer", new { id = Ticket.TicketID });
        }
    }
}

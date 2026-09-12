using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;
using TicketSupportSystem.Data;
using TicketSupportSystem.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.ViewModels;
using TicketSupportSystem.Services;


namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class TicketViewerModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IMessageService _msg;
        public Ticket Ticket {get; set;} = null!;
        public List<MessageView> Messages {get; set;} = new List<MessageView>();
        [BindProperty]
        [Required(ErrorMessage ="Please write a reply before submitting")]
        public required string ReplyText {get; set;} = string.Empty;

        public TicketViewerModel(AppDbContext db, IMessageService msg)
        {
            _db = db;
            _msg = msg;
        }
        public async Task<IActionResult> OnGetAsync(int id)
        {
           var userID = User.GetId();
           if (userID == null) { await HttpContext.SignOutAsync("UserScheme"); return RedirectToPage("/UserView/Login"); }
          
           var ticket = await _db.Tickets.FirstOrDefaultAsync(q => q.UserID == userID &&
                                                                        q.TicketID == id);
           if (ticket == null) { TempData["ErrorMessage"] = "Ticket not found"; return RedirectToPage("/UserView/Dashboard"); }
           Ticket = ticket;
           
           Messages = await _msg.GetPublicMessagesAsync(Ticket.TicketID, userID.Value);
           return Page(); 
        }
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userID = User.GetId();
            if (userID == null) { await HttpContext.SignOutAsync("UserScheme"); return RedirectToPage("/UserView/Login"); }
            
            var ticket = await _db.Tickets.FirstOrDefaultAsync(q => q.UserID == userID && q.TicketID == id);
            if (ticket == null) return RedirectToPage("/UserView/Dashboard");
            Ticket = ticket;   
            
            Messages = await _msg.GetPublicMessagesAsync(Ticket.TicketID, userID.Value);                                      
            
            if (!ModelState.IsValid) { return Page(); }
            var posted = await _msg.AddUserReplyAsync(Ticket.TicketID, userID.Value, ReplyText, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            if (!posted) { TempData["ErrorMessage"] = "Your response could not be submitted"; }
           
            return RedirectToPage("/UserView/TicketViewer", new { id });
        }
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync("UserScheme");
            return RedirectToPage("/UserView/Login");
        }
    }
}

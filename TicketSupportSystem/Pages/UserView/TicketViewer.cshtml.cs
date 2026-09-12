using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using TicketSupportSystem.Data;
using TicketSupportSystem.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.ViewModels;


namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class TicketViewerModel : PageModel
    {
        private readonly AppDbContext _db;
        public Ticket Ticket {get; set;} = null!;
        public List<MessageView> Messages {get; set;} = new List<MessageView>();
        [BindProperty]
        public required string ReplyText {get; set;} = string.Empty;

        public TicketViewerModel(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> OnGetAsync(int id)
        {
           var userID = User.GetId();
           if (userID == null) { await HttpContext.SignOutAsync("UserScheme"); return RedirectToPage("/UserView/Login"); }
           var getTicket = await _db.Tickets.FirstOrDefaultAsync(q => q.UserID == userID &&
                                                                        q.TicketID == id);
           if (getTicket == null) { TempData["ErrorMessage"] = "Ticket not found"; return RedirectToPage("Dashboard"); }
           Ticket = getTicket;
           var messagesResult = await _db.Messages.Where(q => q.IsInternal == false && q.TicketID == id)
                                                        .OrderBy(q => q.PostedAt)
                                                        .Select(m => new MessageView(m.Response, m.PostedAt, m.ResponseByUser != null ? m.ResponseByUser.DisplayName : m.ResponseByStaff.DisplayName))
                                                        .ToListAsync();
            Messages = messagesResult;
            return Page();

            
        }
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userID = User.GetId();
            if (userID == null) { await HttpContext.SignOutAsync("UserScheme"); return RedirectToPage("/UserView/Login"); }
            var getTicket = await _db.Tickets.FirstOrDefaultAsync(q => q.UserID == userID && q.TicketID == id);
            if (getTicket == null) return RedirectToPage("/UserView/Dashboard");
            Ticket = getTicket;   
            var messagesResult = await _db.Messages.Where(q => q.IsInternal == false && q.TicketID == id)
                                                        .OrderBy(q => q.PostedAt)
                                                        .Select(m => new MessageView(m.Response, m.PostedAt, m.ResponseByUser != null ? m.ResponseByUser.DisplayName : m.ResponseByStaff.DisplayName))
                                                        .ToListAsync();    
            Messages = messagesResult;                                       
            if (!ModelState.IsValid) { ModelState.AddModelError("ReplyText", "Please write a reply before submitting"); return Page(); }
                Message message = new Message
                {
                    Response = ReplyText,
                    TicketID = Ticket.TicketID,
                    ResponseByUserID = userID,
                    PostedAt = DateTime.UtcNow,
                    IsInternal = false,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                };
                _db.Messages.Add(message);
                await _db.SaveChangesAsync();
                return RedirectToPage($"/UserView/TicketViewer", new { id });
                }
                
        
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync("UserScheme");
            return RedirectToPage("/UserView/Login");
        }
    }
}

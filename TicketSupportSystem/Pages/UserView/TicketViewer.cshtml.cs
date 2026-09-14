using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;
using TicketSupportSystem.Models;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.ViewModels;
using TicketSupportSystem.Services;


namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class TicketViewerModel : PageModel
    {
        private readonly IMessageService _msg;
        private readonly ITicketService _tk;
        public Ticket Ticket {get; set;} = null!;
        public List<MessageView> Messages {get; set;} = new List<MessageView>();
        [BindProperty]
        [Required(ErrorMessage ="Please write a reply before submitting")]
        [StringLength(5000, ErrorMessage = "The response cannot contain more than 5000 characters")]
        public required string ReplyText {get; set;} = string.Empty;

        public TicketViewerModel(ITicketService tk, IMessageService msg)
        {
            _tk = tk;
            _msg = msg;
        }
        public async Task<IActionResult> OnGetAsync(int id)
        {
           var userID = User.GetId();
           if (userID == null) { return await this.SignOutUserAsync(); }
          
           var ticket = await _tk.GetTicketForUserAsync(id, userID.Value);
           if (ticket == null) { TempData["ErrorMessage"] = "Ticket not found"; return RedirectToPage("/UserView/Dashboard"); }
           Ticket = ticket;
           
           Messages = await _msg.GetPublicMessagesAsync(Ticket.TicketID, userID.Value);
           return Page(); 
        }
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userID = User.GetId();
            if (userID == null) { return await this.SignOutUserAsync(); }
            
            var ticket = await _tk.GetTicketForUserAsync(id, userID.Value);
            if (ticket == null) return Page();
            Ticket = ticket;   
                        
            if (!ModelState.IsValid) { 
                Messages = await _msg.GetPublicMessagesAsync(Ticket.TicketID, userID.Value);                                      
                return Page(); 
                }
            var posted = await _msg.AddUserReplyAsync(Ticket.TicketID, userID.Value, ReplyText, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            if (!posted) { TempData["ErrorMessage"] = "Your response could not be submitted, please try again. If issue persists, try signing out and back in."; }
           
            return RedirectToPage("/UserView/TicketViewer", new { id });
        }
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            return await this.SignOutUserAsync();
        }
    }
}

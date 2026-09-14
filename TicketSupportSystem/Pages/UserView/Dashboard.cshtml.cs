using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.Services;
using TicketSupportSystem.ViewModels;

namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class DashboardModel : PageModel
    {
        private readonly ITicketService _msg;
        public List<TicketSummary> Tickets { get; set; } = new List<TicketSummary>();
        public DashboardModel(ITicketService msg)
        {
            _msg = msg;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var userID = User.GetId();
            if (userID == null){ return await this.SignOutUserAsync(); }
            var TicketResults = await _msg.ListTicketSummariesForUserAsync(userID.Value);
            Tickets = TicketResults;
            return Page();
        }
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            return await this.SignOutUserAsync();
        }
    }
}

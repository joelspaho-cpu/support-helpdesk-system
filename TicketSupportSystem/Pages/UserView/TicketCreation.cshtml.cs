using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSupportSystem.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using TicketSupportSystem.Extensions;
using TicketSupportSystem.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TicketSupportSystem.Pages.UserView
{
    [Authorize(AuthenticationSchemes = "UserScheme")]
    public class TicketCreationModel : PageModel
    {
        private readonly ITicketService _tk;
        private readonly ICategoryService _c;
        public SelectList Categories { get; set; } = default!;
        [BindProperty]
        [Required(ErrorMessage = "Subject cannot be empty"), MaxLength(30)]
        public string Subject {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Description cannot be empty"), MaxLength(5000)]
        public string Description {get; set;} = string.Empty;
        [BindProperty]
        [Required(ErrorMessage = "Please select a category"), Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryID {get; set;}
        public TicketCreationModel (ITicketService tk, ICategoryService c)
        {
            _tk = tk;
            _c = c;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategoriesAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) {
                await LoadCategoriesAsync();
                return Page(); 
                }
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userID = User.GetId();
            if (userID == null) { return await this.SignOutUserAsync(); } 
            var ticketId = await _tk.CreateTicketAsync(userID.Value, Subject, Description, CategoryID, ip);
            if (ticketId == null)
            {
                ModelState.AddModelError(nameof(CategoryID), "Please choose a valid category");
                await LoadCategoriesAsync();
                return Page();
            }
            return RedirectToPage("/UserView/TicketViewer", new { id = ticketId.Value });
        }
        private async Task LoadCategoriesAsync()
        {
            var list = await _c.ListCategoriesAsync();
            Categories = new SelectList(list, nameof(Category.CategoryID), nameof(Category.Name));
        }
    }
}

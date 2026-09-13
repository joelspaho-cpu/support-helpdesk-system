using TicketSupportSystem.Models;
using TicketSupportSystem.ViewModels;
namespace TicketSupportSystem.Services;

public interface ITicketService
{
    Task<Ticket?> GetTicketForUserAsync(int ticketId, int userId);
    Task<List<TicketSummary>> ListTicketSummariesForUserAsync(int userId);
    Task<int> CreateTicketAsync(int userId, string subject, string description, TicketQuery query, string ipAddress);

}
using TicketSupportSystem.Models;
namespace TicketSupportSystem.ViewModels;
public record TicketSummary(int TicketID, string Subject, TicketStatus Status, DateTime CreatedAt, DateTime UpdatedAt);

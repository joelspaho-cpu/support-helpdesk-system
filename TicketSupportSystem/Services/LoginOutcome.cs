using TicketSupportSystem.Models;
namespace TicketSupportSystem.Services;
public record LoginOutcome(LoginResult Result, User? User = null);

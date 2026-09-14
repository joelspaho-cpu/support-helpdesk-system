using TicketSupportSystem.Models;
using System.Security.Claims;

namespace TicketSupportSystem.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<int?> RegisterAsync(string email, string displayName, string password, string region, string language);
    ClaimsPrincipal ConstructPrincipal(User user);
}
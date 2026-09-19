using TicketSupportSystem.Models;
using System.Security.Claims;

namespace TicketSupportSystem.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<int?> RegisterAsync(string displayName, string email, string password, string region, string language, bool has2fa);
    Task<int?> CreateVerifiedUserAsync(PendingRegistration pending);
    ClaimsPrincipal ConstructPrincipal(User user);
}
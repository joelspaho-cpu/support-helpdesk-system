using TicketSupportSystem.Models;
using System.Security.Claims;

namespace TicketSupportSystem.Services;

public interface IUserService
{
    Task<LoginOutcome> AuthenticateAsync(string email, string password);
    Task<int?> RegisterAsync(string displayName, string email, string password, string region, string language, bool has2fa);
    Task<int?> CreateVerifiedUserAsync(PendingRegistration pending);
    ClaimsPrincipal ConstructPrincipal(int UserID);
    Task<UserSettings?> GetSettingsAsync(int id);
    Task<SettingsUpdateResult> ChangeSettingsAsync(int id, string? currentPassword, bool Has2Fa, string? DisplayName = null, string? Region = null, string? Language = null, string? newPassword = null);
}
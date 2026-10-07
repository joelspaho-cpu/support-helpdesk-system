using TicketSupportSystem.Models;

namespace TicketSupportSystem.Services;

public interface IVerificationService
{
    Task<Guid?> StartAsync(string displayName, string email, string password, string region, string language, bool has2fa);
    Task<VerificationResult> RegisterVerifyAsync(Guid id, int enteredCode);
    Task<ResendResult> ResendAsync(Guid id);
    Task<ResendStatus> GetResendStatusAsync(Guid id);
    Task<Guid?> StartLoginAsync(int userId, bool isPersistent);
    Task<TwoFactorOutcome> TwoFactorVerifyAsync(Guid id, int enteredCode);
    Task<ResendStatus> GetResendStatusLoginAsync(Guid id);
    Task<ResendResult>ResendLoginAsync(Guid id);
}
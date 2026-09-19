using TicketSupportSystem.Models;

namespace TicketSupportSystem.Services;

public interface IVerificationService
{
    Task<Guid?> StartAsync(string displayName, string email, string password, string region, string language, bool has2fa);
    Task<VerificationResult> VerifyAsync(Guid id, int enteredCode);
    Task<ResendResult> ResendAsync(Guid id);
    Task<bool> CanResendAsync(Guid id);
}
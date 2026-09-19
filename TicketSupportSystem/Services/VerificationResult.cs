namespace TicketSupportSystem.Services;

public enum VerificationResult
{
    NotFound,
    Expired,
    TooManyAttempts,
    InvalidCode,
    Success
}
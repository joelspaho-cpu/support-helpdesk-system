using TicketSupportSystem.Models;

namespace TicketSupportSystem.Services; 

public record TwoFactorOutcome(VerificationResult Result, int? UserID = null, bool IsPersistent = false);
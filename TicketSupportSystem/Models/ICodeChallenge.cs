namespace TicketSupportSystem.Models;

public interface ICodeChallenge
{
    string CodeHash { get; set; }
    DateTime ExpiresAt { get; set; }
    int AttemptCount { get; set; }
}
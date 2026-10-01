using System.ComponentModel.DataAnnotations;

namespace TicketSupportSystem.Models;

public class PendingLogin : ICodeChallenge
{
    public Guid ID {get; set;}
    public User? User {get; set;} 
    public int UserID {get; set;}
    [MaxLength(100)]
    public required string CodeHash {get; set;}
    public DateTime ExpiresAt {get; set;}
    public int AttemptCount {get; set;}
    public DateTime CreatedAt {get; set;}
    public bool IsPersistent {get; set;}
}
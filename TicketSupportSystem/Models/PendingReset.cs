using System.ComponentModel.DataAnnotations;
namespace TicketSupportSystem.Models;

public class PendingReset { 
    public Guid Id {get; set;}
    public int UserID {get; set;}
    public User? User {get; set;}
    [MaxLength(100)]
    public required string TokenHash {get; set;}
    public DateTime ExpiresAt {get; set;}
    public DateTime CreatedAt {get; set;}
}

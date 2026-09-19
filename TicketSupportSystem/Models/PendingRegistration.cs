using System.ComponentModel.DataAnnotations;
namespace TicketSupportSystem.Models;

public class PendingRegistration {
    public Guid Id {get; set;}
    [Required, MaxLength(50)]
    public required string DisplayName {get; set;}
    [Required, EmailAddress, MaxLength(254)]
    public required string Email {get; set;}
    [MaxLength(100)]
    public required string PasswordHash {get; set;}
    [MaxLength(100)]
    public required string CodeHash {get; set;}
    public bool Has2fa {get; set;}
    [Required, MaxLength(10)]
    public required string Region {get; set;}
    [Required, MaxLength(10)]
    public required string Language {get; set;}
    public DateTime CreatedAt {get; set;}
    public int AttemptCount {get; set;}
    public DateTime ExpiresAt {get; set;}
}
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TicketSupportSystem.Models;

public class Staff : IdentityUser<int>{
    [Required, MaxLength(50)]
    public required string DisplayName {get; set;}
    public Level? Level {get; set;}
    [MaxLength(100)]
    public string? Signature {get; set;}
    public DateTime CreatedAt {get; set;}
}
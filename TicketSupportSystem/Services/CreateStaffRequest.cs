using TicketSupportSystem.Models;

namespace TicketSupportSystem.Services;
public class CreateStaffRequest
{
    public required string Email {get; init;}
    public required string DisplayName {get; init;}
    public required string Password {get; init;}
    public required string Role {get; init;}
    public Level? Level {get; init;}
    public string? Signature {get; init;}
}
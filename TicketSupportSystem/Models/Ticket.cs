using System.ComponentModel.DataAnnotations;

namespace TicketSupportSystem.Models;

public class Ticket
{
    public int TicketID {get; set;}
    public int UserID {get; set;}
    public User? User {get; set;}
    public Staff? AssignedToStaff {get; set;}
    public int? AssignedToStaffID {get; set;}
    public Level Level {get; set;}
    public TicketPriority Priority {get; set;}
    [Required, MaxLength(30)]
    public required string Subject {get; set;}
    [Required, MaxLength(5000)]
    public required string Description {get; set;}
    public TicketStatus Status {get; set;}
    [Required]
    public TicketQuery Query {get; set;}
    public int? ProductID {get; set;}
    [Required, MaxLength(45)]
    public required string IPAddress {get; set;}
    public DateTime? UpdatedAt {get; set;}
    public Staff? ReferredToEngineer { get; set; }
    public int? ReferredToEngineerID { get; set; } 
    public DateTime CreatedAt {get; set;}
    
}

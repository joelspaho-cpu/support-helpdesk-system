using System.ComponentModel.DataAnnotations;

namespace TicketSupportSystem.Models;

public class Category
{
    public int CategoryID {get; set;}
    [MaxLength(100)]
    public required string Name {get; set;}
    public Level HandledByLevel {get; set;}
    public TicketPriority DefaultPriority {get; set;}
    public bool IsEnabled {get; set;}
    public int SortOrder {get; set;}
}
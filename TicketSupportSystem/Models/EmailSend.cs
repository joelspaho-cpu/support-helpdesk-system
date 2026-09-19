using System.ComponentModel.DataAnnotations;

namespace TicketSupportSystem.Models;

public class EmailSend
{
    public int EmailSendId { get; set; }
    [MaxLength(254)]
    public required string Email {get; set;}
    public DateTime Expiry {get; set;}
    public int ResendCount {get; set;}
}
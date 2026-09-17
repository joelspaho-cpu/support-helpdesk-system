using Microsoft.Extensions.Options;

namespace TicketSupportSystem.Services;

public class EmailOptions
{
    public string Host {get; set;} = string.Empty;
    public int Port {get; set;}
    public string Password {get; set;} = string.Empty;
    public string FromName {get; set;} = string.Empty;
    public string FromAddress {get; set;} = string.Empty;
    public string Username {get; set;} = string.Empty;

    
}
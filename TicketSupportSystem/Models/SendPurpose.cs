namespace TicketSupportSystem.Models;
public enum SendPurpose
{
    Verification,   // registration and login codes. First, so existing rows (value 0) get this.
    Reset
}

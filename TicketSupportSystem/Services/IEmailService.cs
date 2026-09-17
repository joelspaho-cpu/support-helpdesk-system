namespace TicketSupportSystem.Services;

public interface IEmailService{
public Task SendAsync(string to, string subject, string htmlBody);

}
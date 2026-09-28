using TicketSupportSystem.Services;

namespace TicketSupportSystem.Tests.Fakes;

public class FakeEmailService : IEmailService
{
    public List<(string To, string Subject, string Body)> Sent { get; } = new();

    public Task SendAsync(string to, string subject, string htmlBody)
    {
        Sent.Add((to, subject, htmlBody));
        return Task.CompletedTask;
    }
}

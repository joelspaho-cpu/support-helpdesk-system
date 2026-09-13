using TicketSupportSystem.Models;
using TicketSupportSystem.Data;
using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.ViewModels;

namespace TicketSupportSystem.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    public TicketService(AppDbContext db)
    {
        _db = db;
    }
    public async Task<Ticket?> GetTicketForUserAsync(int ticketId, int userId)
    {
        return await _db.Tickets.FirstOrDefaultAsync(t => t.TicketID == ticketId && t.UserID == userId);
    }
    public async Task<List<TicketSummary>> ListTicketSummariesForUserAsync(int userId)
    {
        return await _db.Tickets
            .Where(u => u.UserID == userId)
            .OrderByDescending(q => q.CreatedAt)
            .Select(u => new TicketSummary(u.TicketID, u.Subject, u.Status, u.CreatedAt, u.UpdatedAt ?? u.CreatedAt))
            .ToListAsync();
    }
    public async Task<int> CreateTicketAsync(int userId, string subject, string description, TicketQuery query, string ipAddress)
    {   
        var level = query switch
        {
            TicketQuery.GeneralQuestion => Level.Level1,
            TicketQuery.Billing => Level.Level1,
            TicketQuery.Account => Level.Level2,
            TicketQuery.Product => Level.Level2,
            TicketQuery.FeedbackComplaint => Level.Level2,
            TicketQuery.Sales => Level.Level1,
            _ => throw new ArgumentOutOfRangeException(nameof(query))
        };
        Ticket ticket = new Ticket
        {
            UserID = userId,
            Subject = subject,
            Description = description,
            Query = query,
            Status = TicketStatus.Open,
            Level = level, 
            IPAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket.TicketID;
    }

}
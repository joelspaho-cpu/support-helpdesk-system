using TicketSupportSystem.Models;
using TicketSupportSystem.Data;
using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.ViewModels;

namespace TicketSupportSystem.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    private readonly ICategoryService _c;
    public TicketService(AppDbContext db, ICategoryService c)
    {
        _db = db;
        _c = c;
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
    public async Task<int?> CreateTicketAsync(int userId, string subject, string description, int category, string ipAddress)
    {   
        var result = await _c.GetCategoryAsync(category);
        if (result == null) return null;
        Ticket ticket = new Ticket
        {
            UserID = userId,
            Subject = subject,
            Description = description,
            CategoryID = result.CategoryID,
            Status = TicketStatus.Open,
            Level = result.HandledByLevel, 
            IPAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            Priority = result.DefaultPriority
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket.TicketID;
    }

}
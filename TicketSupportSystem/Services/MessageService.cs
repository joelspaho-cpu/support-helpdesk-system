using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.ViewModels;
using TicketSupportSystem.Data;
using TicketSupportSystem.Models;
namespace TicketSupportSystem.Services;
public class MessageService : IMessageService
{
   private readonly AppDbContext _db;
   public MessageService(AppDbContext db)
    {
        _db = db;
    }
   public async Task<List<MessageView>> GetPublicMessagesAsync(int ticketId, int userId)
    {
        return await _db.Messages
            .Where(m => m.TicketID == ticketId && m.IsInternal == false && m.Ticket!.UserID == userId)
            .OrderBy(m => m.PostedAt)
            .Select(m => new MessageView(
                m.Response,
                m.PostedAt,
                m.ResponseByStaff != null ? m.ResponseByStaff.DisplayName : m.ResponseByUser!.DisplayName))
            .ToListAsync();
    }
   public async Task<bool> AddUserReplyAsync(int ticketId, int userId, string text, string ipAddress)
    {
        var owns = await _db.Tickets.AnyAsync(t => t.TicketID == ticketId && t.UserID == userId);
        if (!owns) return false;
        Message message = new Message
        {
            TicketID = ticketId,
            Response = text,
            ResponseByUserID = userId,
            IPAddress = ipAddress,
            PostedAt = DateTime.UtcNow,
            IsInternal = false,
        };
        _db.Messages.Add(message);
        await _db.SaveChangesAsync();
        return true;
    }
}
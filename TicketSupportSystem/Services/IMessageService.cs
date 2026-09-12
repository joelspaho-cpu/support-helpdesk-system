using TicketSupportSystem.ViewModels;
namespace TicketSupportSystem.Services;
public interface IMessageService
{
   Task<List<MessageView>> GetPublicMessagesAsync(int ticketId, int userId);
   Task<bool> AddUserReplyAsync(int ticketId, int userId, string text, string ipAddress);

}
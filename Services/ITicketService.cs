namespace TaskFlow.Services;

public interface ITicketService
{
    Task<IEnumerable<Ticket>> GetAllAsync(int? projectId = null);
    Task<Ticket?> GetByKeyAsync(string key);
    Task<Ticket?> GetByIdAsync(int id);
    Task<Ticket> CreateAsync(Ticket ticket);
    Task<Ticket?> UpdateAsync(int id, string? title, string? description, TicketType? type, int? statusId, TicketPriority? priority, int? assigneeId);
    Task<bool> DeleteAsync(int id);
    Task<Comment> AddCommentAsync(int ticketId, int authorId, string body);
}

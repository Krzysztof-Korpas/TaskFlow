using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace TaskFlow.Services;

public class TicketService(ApplicationDbContext db, IRabbitMqService mq) : ITicketService
{
    private readonly ApplicationDbContext _db = db;
    private readonly IRabbitMqService _mq = mq;
    public async Task<IEnumerable<Ticket>> GetAllAsync(int? projectId = null)
    {
        IQueryable<Ticket> q = _db.Tickets
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Reporter)
            .AsQueryable();
        if (projectId.HasValue)
            q = q.Where(t => t.ProjectId == projectId.Value);
        return await q.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<Ticket?> GetByKeyAsync(string key) =>
        await _db.Tickets
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Reporter)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Key == key);

    public async Task<Ticket?> GetByIdAsync(int id) =>
        await _db.Tickets
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .Include(t => t.Reporter)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            bool useTransaction = _db.Database.IsRelational();
            await using IDbContextTransaction? tx = useTransaction
                ? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
                : null;
            int seq = await _db.Tickets
                .Where(t => t.ProjectId == ticket.ProjectId)
                .CountAsync() + 1;
            Project? project = await _db.Projects.FindAsync(ticket.ProjectId);
            ticket.Key = $"{project?.Key ?? "PRJ"}-{seq}";
            ticket.CreatedAt = DateTime.UtcNow;
            _db.Tickets.Add(ticket);
            try
            {
                await _db.SaveChangesAsync();
                if (useTransaction && tx != null)
                    await tx.CommitAsync();
                _mq.Publish("stc.tickets", "ticket.created", System.Text.Json.JsonSerializer.Serialize(new
                {
                    ticket.Id,
                    ticket.Key,
                    ticket.Title,
                    ticket.ProjectId,
                    ticket.ReporterId,
                    ticket.AssigneeId
                }));
                return ticket;
            }
            catch (DbUpdateException) when (attempt < 3)
            {
                if (useTransaction && tx != null)
                    await tx.RollbackAsync();
                _db.Entry(ticket).State = EntityState.Detached;
                ticket.Id = 0;
            }
        }

        throw new InvalidOperationException("Failed to create ticket after retries.");
    }

    public async Task<Ticket?> UpdateAsync(int id, string? title, string? description, TicketType? type, TicketStatus? status, TicketPriority? priority, int? assigneeId)
    {
        Ticket? ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null) return null;
        if (title != null) ticket.Title = title;
        if (description != null) ticket.Description = description;
        if (type.HasValue) ticket.Type = type.Value;
        if (status.HasValue) ticket.Status = status.Value;
        if (priority.HasValue) ticket.Priority = priority.Value;
        if (assigneeId.HasValue) ticket.AssigneeId = assigneeId.Value == 0 ? null : assigneeId;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _mq.Publish("stc.tickets", "ticket.updated", System.Text.Json.JsonSerializer.Serialize(new
        {
            ticket.Id,
            ticket.Key,
            ticket.Status,
            ticket.AssigneeId
        }));
        return ticket;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Ticket? ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null) return false;
        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
        _mq.Publish("stc.tickets", "ticket.deleted", System.Text.Json.JsonSerializer.Serialize(new { Id = id, ticket.Key }));
        return true;
    }

    public async Task<Comment> AddCommentAsync(int ticketId, int authorId, string body)
    {
        Comment comment = new Comment { TicketId = ticketId, AuthorId = authorId, Body = body };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        _mq.Publish("stc.tickets", "ticket.commented", System.Text.Json.JsonSerializer.Serialize(new
        {
            comment.Id,
            ticketId,
            authorId
        }));
        await _db.Entry(comment).Reference(c => c.Author).LoadAsync();
        return comment;
    }
}

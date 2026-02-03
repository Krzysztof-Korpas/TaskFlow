namespace TaskFlow.Models;

public class Ticket
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketType Type { get; set; } = TicketType.Task;
    public int StatusId { get; set; }
    public ProjectStatus Status { get; set; } = null!;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }

    public int ReporterId { get; set; }
    public ApplicationUser Reporter { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = [];
}

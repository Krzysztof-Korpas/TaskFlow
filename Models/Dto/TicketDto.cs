namespace TaskFlow.Models.Dto;

public class TicketDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectKey { get; set; }
    public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public int ReporterId { get; set; }
    public string? ReporterName { get; set; }
    public List<CommentDto> Comments { get; set; } = [];
}

public class CommentDto
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
}

public class CreateTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProjectId { get; set; }
    public int ReporterId { get; set; }
    public int? AssigneeId { get; set; }
    public TicketType Type { get; set; } = TicketType.Task;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}

public class UpdateTicketRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TicketType? Type { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public int? AssigneeId { get; set; }
}

public class AddCommentRequest
{
    public string Body { get; set; } = string.Empty;
}

using TaskFlow.Models.Dto;

namespace TaskFlow.Services;

public static class MappingExtensions
{
    public static TicketDto ToDto(this Ticket t) => new()
    {
        Id = t.Id,
        Key = t.Key,
        Title = t.Title,
        Description = t.Description,
        Type = t.Type,
        Status = t.Status,
        Priority = t.Priority,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        ProjectId = t.ProjectId,
        ProjectKey = t.Project?.Key,
        AssigneeId = t.AssigneeId,
        AssigneeName = t.Assignee?.DisplayName,
        ReporterId = t.ReporterId,
        ReporterName = t.Reporter?.DisplayName,
        Comments = t.Comments?.Select(c => c.ToDto()).ToList() ?? new()
    };

    public static CommentDto ToDto(this Comment c) => new()
    {
        Id = c.Id,
        Body = c.Body,
        CreatedAt = c.CreatedAt,
        AuthorId = c.AuthorId,
        AuthorName = c.Author?.DisplayName
    };

    public static ProjectDto ToDto(this Project p, int ticketCount = 0) => new()
    {
        Id = p.Id,
        Key = p.Key,
        Name = p.Name,
        Description = p.Description,
        CreatedAt = p.CreatedAt,
        TicketCount = ticketCount
    };

    public static UserDto ToDto(this ApplicationUser u) => new()
    {
        Id = u.Id,
        Email = u.Email ?? string.Empty,
        DisplayName = u.DisplayName,
        AvatarUrl = u.AvatarUrl
    };
}

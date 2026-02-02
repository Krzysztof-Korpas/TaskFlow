using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class Project
{
    public int Id { get; init; }
    [MaxLength(130)]
    public string Key { get; set; } = string.Empty;
    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(600)]
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Ticket> Tickets { get; set; } = [];
    public ICollection<ProjectUserGroup> AssignedUsers { get; set; } = [];
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Models;

public class ApplicationUser : IdentityUser<int>
{
    [MaxLength(180)]
    public string DisplayName { get; init; } = string.Empty;
    [MaxLength(200)]
    public string? AvatarUrl { get; init; }

    public ICollection<Ticket> AssignedTickets { get; set; } = [];
    public ICollection<Ticket> CreatedTickets { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<ProjectUserGroup> ProjectAssignments { get; set; } = [];
    public ICollection<KanbanColumnPreference> KanbanColumnPreferences { get; set; } = [];
}

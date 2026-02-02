using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class ProjectUserGroup
{
    public int Id { get; init; }
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

namespace TaskFlow.Models;

public class KanbanColumnPreference
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int StatusId { get; set; }
    public ProjectStatus Status { get; set; } = null!;

    public int Position { get; set; }
    public bool IsVisible { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Models;

public class ProjectStatus
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = [];
    public ICollection<KanbanColumnPreference> ColumnPreferences { get; set; } = [];
}

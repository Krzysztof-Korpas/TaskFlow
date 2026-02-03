namespace TaskFlow.Models.Dto;

public class ProjectStatusDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
}

public class KanbanColumnDto
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool IsVisible { get; set; }
}

public class SaveKanbanColumnsRequest
{
    public List<KanbanColumnDto> Columns { get; set; } = [];
}

public class CreateProjectStatusRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateProjectStatusRequest
{
    public string Name { get; set; } = string.Empty;
}

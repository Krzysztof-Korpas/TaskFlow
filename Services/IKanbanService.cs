using TaskFlow.Models;

namespace TaskFlow.Services;

public interface IKanbanService
{
    Task<List<ProjectStatus>> GetProjectStatusesAsync(int projectId);
    Task<ProjectStatus?> GetStatusAsync(int projectId, int statusId);
    Task<ProjectStatus> CreateStatusAsync(int projectId, string name);
    Task<ProjectStatus?> UpdateStatusAsync(int projectId, int statusId, string name);
    Task<bool> DeleteStatusAsync(int projectId, int statusId);
    Task<int> GetDefaultStatusIdAsync(int projectId);
    Task<List<KanbanColumnPreference>> GetOrCreateUserColumnsAsync(int projectId, int userId);
    Task SaveUserColumnsAsync(int projectId, int userId, List<KanbanColumnPreference> columns);
}

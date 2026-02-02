namespace TaskFlow.Services;

using TaskFlow.Models;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetAllForUserAsync(int userId, bool isAdmin);
    Task<Project?> GetByIdAsync(int id);
    Task<Project> CreateAsync(Project project);
    Task<Project?> UpdateAsync(int id, string? name, string? description);
    Task<bool> DeleteAsync(int id);
    Task<bool> AddUserToProjectAsync(int projectId, int userId);
    Task<bool> RemoveUserFromProjectAsync(int projectId, int userId);
    Task<IEnumerable<ApplicationUser>> GetProjectUsersAsync(int projectId);
    Task<bool> UserHasAccessToProjectAsync(int projectId, int userId, bool isAdmin);
}

using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Services;

public class ProjectService(ApplicationDbContext db) : IProjectService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<IEnumerable<Project>> GetAllAsync() =>
        await _db.Projects.OrderBy(p => p.Name).ToListAsync();

    public async Task<IEnumerable<Project>> GetAllForUserAsync(int userId, bool isAdmin)
    {
        if (isAdmin)
            return await GetAllAsync();

        return await _db.Projects
            .Where(p => p.AssignedUsers.Any(au => au.UserId == userId))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id) =>
        await _db.Projects.Include(p => p.Tickets).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Project> CreateAsync(Project project)
    {
        project.Key = project.Key.ToUpperInvariant();
        if (await _db.Projects.AnyAsync(p => p.Key == project.Key))
            throw new InvalidOperationException($"Project key '{project.Key}' already exists.");
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return project;
    }

    public async Task<Project?> UpdateAsync(int id, string? name, string? description)
    {
        Project? p = await _db.Projects.FindAsync(id);
        if (p == null) return null;
        if (name != null) p.Name = name;
        if (description != null) p.Description = description;
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Project? p = await _db.Projects.FindAsync(id);
        if (p == null) return false;
        _db.Projects.Remove(p);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddUserToProjectAsync(int projectId, int userId)
    {
        bool exists = await _db.ProjectUserGroups
            .AnyAsync(pug => pug.ProjectId == projectId && pug.UserId == userId);
        
        if (exists) return false;

        ProjectUserGroup projectUserGroup = new ()
        {
            ProjectId = projectId,
            UserId = userId
        };

        _db.ProjectUserGroups.Add(projectUserGroup);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveUserFromProjectAsync(int projectId, int userId)
    {
        ProjectUserGroup? assignment = await _db.ProjectUserGroups
            .FirstOrDefaultAsync(pug => pug.ProjectId == projectId && pug.UserId == userId);
        
        if (assignment == null) return false;

        _db.ProjectUserGroups.Remove(assignment);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ApplicationUser>> GetProjectUsersAsync(int projectId)
    {
        return await _db.ProjectUserGroups
            .Where(pug => pug.ProjectId == projectId)
            .Include(pug => pug.User)
            .Select(pug => pug.User)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }

    public async Task<bool> UserHasAccessToProjectAsync(int projectId, int userId, bool isAdmin)
    {
        if (isAdmin) return true;

        return await _db.ProjectUserGroups
            .AnyAsync(pug => pug.ProjectId == projectId && pug.UserId == userId);
    }
}

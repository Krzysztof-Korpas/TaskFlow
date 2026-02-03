using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Models;

namespace TaskFlow.Services;

public class KanbanService(ApplicationDbContext db) : IKanbanService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<List<ProjectStatus>> GetProjectStatusesAsync(int projectId) =>
        await _db.ProjectStatuses
            .Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();

    public async Task<ProjectStatus?> GetStatusAsync(int projectId, int statusId) =>
        await _db.ProjectStatuses.FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Id == statusId);

    public async Task<ProjectStatus> CreateStatusAsync(int projectId, string name)
    {
        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Status name is required.");

        bool exists = await _db.ProjectStatuses.AnyAsync(s => s.ProjectId == projectId && s.Name.ToLower() == trimmed.ToLower());
        if (exists) throw new InvalidOperationException("Status name already exists.");

        int nextOrder = await _db.ProjectStatuses
            .Where(s => s.ProjectId == projectId)
            .Select(s => (int?)s.SortOrder)
            .MaxAsync() ?? -1;

        ProjectStatus status = new()
        {
            ProjectId = projectId,
            Name = trimmed,
            SortOrder = nextOrder + 1,
            IsDefault = false
        };
        _db.ProjectStatuses.Add(status);
        await _db.SaveChangesAsync();
        return status;
    }

    public async Task<ProjectStatus?> UpdateStatusAsync(int projectId, int statusId, string name)
    {
        ProjectStatus? status = await GetStatusAsync(projectId, statusId);
        if (status == null) return null;

        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("Status name is required.");

        bool exists = await _db.ProjectStatuses.AnyAsync(s =>
            s.ProjectId == projectId && s.Id != statusId && s.Name.ToLower() == trimmed.ToLower());
        if (exists) throw new InvalidOperationException("Status name already exists.");

        status.Name = trimmed;
        await _db.SaveChangesAsync();
        return status;
    }

    public async Task<bool> DeleteStatusAsync(int projectId, int statusId)
    {
        ProjectStatus? status = await GetStatusAsync(projectId, statusId);
        if (status == null) return false;
        bool hasTickets = await _db.Tickets.AnyAsync(t => t.ProjectId == projectId && t.StatusId == statusId);
        if (hasTickets) throw new InvalidOperationException("Status in use.");
        _db.ProjectStatuses.Remove(status);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetDefaultStatusIdAsync(int projectId)
    {
        ProjectStatus? status = await _db.ProjectStatuses
            .Where(s => s.ProjectId == projectId && s.IsDefault)
            .OrderBy(s => s.SortOrder)
            .FirstOrDefaultAsync();
        if (status != null) return status.Id;

        ProjectStatus? first = await _db.ProjectStatuses
            .Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.SortOrder)
            .FirstOrDefaultAsync();
        if (first == null) throw new InvalidOperationException("Project has no statuses.");
        return first.Id;
    }

    public async Task<List<KanbanColumnPreference>> GetOrCreateUserColumnsAsync(int projectId, int userId)
    {
        List<ProjectStatus> statuses = await GetProjectStatusesAsync(projectId);
        List<KanbanColumnPreference> prefs = await _db.KanbanColumnPreferences
            .Where(p => p.ProjectId == projectId && p.UserId == userId)
            .ToListAsync();

        int maxPosition = prefs.Count == 0 ? -1 : prefs.Max(p => p.Position);
        bool added = false;
        foreach (ProjectStatus status in statuses)
        {
            if (prefs.Any(p => p.StatusId == status.Id)) continue;
            prefs.Add(new KanbanColumnPreference
            {
                ProjectId = projectId,
                UserId = userId,
                StatusId = status.Id,
                Position = ++maxPosition,
                IsVisible = true
            });
            added = true;
        }

        if (added)
        {
            _db.KanbanColumnPreferences.AddRange(prefs.Where(p => p.Id == 0));
            await _db.SaveChangesAsync();
        }

        return prefs.OrderBy(p => p.Position).ToList();
    }

    public async Task SaveUserColumnsAsync(int projectId, int userId, List<KanbanColumnPreference> columns)
    {
        List<ProjectStatus> statuses = await GetProjectStatusesAsync(projectId);
        HashSet<int> statusIds = statuses.Select(s => s.Id).ToHashSet();

        List<KanbanColumnPreference> existing = await _db.KanbanColumnPreferences
            .Where(p => p.ProjectId == projectId && p.UserId == userId)
            .ToListAsync();

        int maxPosition = columns.Count == 0 ? -1 : columns.Max(c => c.Position);

        foreach (ProjectStatus status in statuses)
        {
            KanbanColumnPreference? incoming = columns.FirstOrDefault(c => c.StatusId == status.Id);
            KanbanColumnPreference? current = existing.FirstOrDefault(p => p.StatusId == status.Id);
            if (incoming == null)
            {
                if (current == null)
                {
                    existing.Add(new KanbanColumnPreference
                    {
                        ProjectId = projectId,
                        UserId = userId,
                        StatusId = status.Id,
                        Position = ++maxPosition,
                        IsVisible = false
                    });
                }
                else
                {
                    current.IsVisible = false;
                    if (current.Position < 0)
                        current.Position = ++maxPosition;
                }
                continue;
            }

            if (!statusIds.Contains(incoming.StatusId)) continue;

            if (current == null)
            {
                existing.Add(new KanbanColumnPreference
                {
                    ProjectId = projectId,
                    UserId = userId,
                    StatusId = incoming.StatusId,
                    Position = incoming.Position,
                    IsVisible = incoming.IsVisible
                });
            }
            else
            {
                current.Position = incoming.Position;
                current.IsVisible = incoming.IsVisible;
            }
        }

        List<KanbanColumnPreference> newEntries = existing.Where(p => p.Id == 0).ToList();
        if (newEntries.Count > 0)
            _db.KanbanColumnPreferences.AddRange(newEntries);

        await _db.SaveChangesAsync();
    }
}

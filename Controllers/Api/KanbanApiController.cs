using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models.Dto;

namespace TaskFlow.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KanbanApiController(IKanbanService kanbanService, IProjectService projectService, UserManager<ApplicationUser> userManager) : ControllerBase
{
    private readonly IKanbanService _kanbanService = kanbanService;
    private readonly IProjectService _projectService = projectService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private async Task<(ApplicationUser? User, bool IsAdmin, ActionResult? Error)> GetUserAndCheckAccess(int projectId)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return (null, false, Unauthorized());
        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool hasAccess = await _projectService.UserHasAccessToProjectAsync(projectId, currentUser.Id, isAdmin);
        if (!hasAccess) return (currentUser, isAdmin, Forbid());
        return (currentUser, isAdmin, null);
    }

    [HttpGet("project/{projectId:int}/statuses")]
    public async Task<ActionResult<IEnumerable<ProjectStatusDto>>> GetStatuses(int projectId)
    {
        var result = await GetUserAndCheckAccess(projectId);
        if (result.Error != null) return result.Error;

        List<ProjectStatus> statuses = await _kanbanService.GetProjectStatusesAsync(projectId);
        return Ok(statuses.Select(s => new ProjectStatusDto
        {
            Id = s.Id,
            ProjectId = s.ProjectId,
            Name = s.Name,
            SortOrder = s.SortOrder,
            IsDefault = s.IsDefault
        }));
    }

    [HttpPost("project/{projectId:int}/statuses")]
    public async Task<ActionResult<ProjectStatusDto>> CreateStatus(int projectId, [FromBody] CreateProjectStatusRequest req)
    {
        var result = await GetUserAndCheckAccess(projectId);
        if (result.Error != null) return result.Error;

        try
        {
            ProjectStatus status = await _kanbanService.CreateStatusAsync(projectId, req.Name);
            return Ok(new ProjectStatusDto
            {
                Id = status.Id,
                ProjectId = status.ProjectId,
                Name = status.Name,
                SortOrder = status.SortOrder,
                IsDefault = status.IsDefault
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("project/{projectId:int}/statuses/{statusId:int}")]
    public async Task<ActionResult<ProjectStatusDto>> UpdateStatus(int projectId, int statusId, [FromBody] UpdateProjectStatusRequest req)
    {
        var result = await GetUserAndCheckAccess(projectId);
        if (result.Error != null) return result.Error;

        try
        {
            ProjectStatus? status = await _kanbanService.UpdateStatusAsync(projectId, statusId, req.Name);
            if (status == null) return NotFound();
            return Ok(new ProjectStatusDto
            {
                Id = status.Id,
                ProjectId = status.ProjectId,
                Name = status.Name,
                SortOrder = status.SortOrder,
                IsDefault = status.IsDefault
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("project/{projectId:int}/statuses/{statusId:int}")]
    public async Task<ActionResult> DeleteStatus(int projectId, int statusId)
    {
        var result = await GetUserAndCheckAccess(projectId);
        if (result.Error != null) return result.Error;

        try
        {
            bool deleted = await _kanbanService.DeleteStatusAsync(projectId, statusId);
            if (!deleted) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("project/{projectId:int}/columns")]
    public async Task<ActionResult<IEnumerable<KanbanColumnDto>>> GetColumns(int projectId)
    {
        var result = await GetUserAndCheckAccess(projectId);
        if (result.Error != null) return result.Error;

        List<KanbanColumnPreference> prefs = await _kanbanService.GetOrCreateUserColumnsAsync(projectId, result.User!.Id);
        List<ProjectStatus> statuses = await _kanbanService.GetProjectStatusesAsync(projectId);
        Dictionary<int, string> statusNames = statuses.ToDictionary(s => s.Id, s => s.Name);

        return Ok(prefs
            .OrderBy(p => p.Position)
            .Select(p => new KanbanColumnDto
            {
                StatusId = p.StatusId,
                StatusName = statusNames.TryGetValue(p.StatusId, out string? name) ? name : "Unknown",
                Position = p.Position,
                IsVisible = p.IsVisible
            }));
    }

    [HttpPut("project/{projectId:int}/columns")]
    public async Task<ActionResult<IEnumerable<KanbanColumnDto>>> SaveColumns(int projectId, [FromBody] SaveKanbanColumnsRequest req)
    {
        var result = await GetUserAndCheckAccess(projectId);
        if (result.Error != null) return result.Error;

        List<KanbanColumnPreference> incoming = req.Columns.Select(c => new KanbanColumnPreference
        {
            ProjectId = projectId,
            UserId = result.User!.Id,
            StatusId = c.StatusId,
            Position = c.Position,
            IsVisible = c.IsVisible
        }).ToList();

        await _kanbanService.SaveUserColumnsAsync(projectId, result.User!.Id, incoming);
        return await GetColumns(projectId);
    }
}

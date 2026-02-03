using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models.Dto;

namespace TaskFlow.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsApiController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsApiController(IProjectService projectService, UserManager<ApplicationUser> userManager)
    {
        _projectService = projectService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll()
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        IEnumerable<Project> list = await _projectService.GetAllForUserAsync(currentUser.Id, isAdmin);
        
        List<ProjectDto> dtos = list.Select(p => p.ToDto(0)).ToList();
        foreach (ProjectDto d in dtos)
        {
            Project? proj = await _projectService.GetByIdAsync(d.Id);
            if (proj != null) d.TicketCount = proj.Tickets?.Count ?? 0;
        }
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> GetById(int id)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool hasAccess = await _projectService.UserHasAccessToProjectAsync(id, currentUser.Id, isAdmin);
        
        if (!hasAccess) return Forbid();

        Project? p = await _projectService.GetByIdAsync(id);
        if (p == null) return NotFound();
        return Ok(p.ToDto(p.Tickets?.Count ?? 0));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectRequest req)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        if (!isAdmin) return Forbid();

        try
        {
            Project project = new() { Key = req.Key, Name = req.Name, Description = req.Description };
            Project created = await _projectService.CreateAsync(project);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto(0));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] UpdateProjectRequest req)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        if (!isAdmin) return Forbid();

        Project? p = await _projectService.UpdateAsync(id, req.Name, req.Description);
        if (p == null) return NotFound();
        Project? proj = await _projectService.GetByIdAsync(id);
        return Ok(p.ToDto(proj?.Tickets?.Count ?? 0));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        if (!isAdmin) return Forbid();

        if (!await _projectService.DeleteAsync(id)) return NotFound();
        return NoContent();
    }

    [HttpGet("{id:int}/users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetProjectUsers(int id)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        if (!isAdmin) return Forbid();

        IEnumerable<ApplicationUser> users = await _projectService.GetProjectUsersAsync(id);
        return Ok(users.Select(u => u.ToDto()));
    }

    [HttpPost("{id:int}/users/{userId:int}")]
    public async Task<ActionResult> AddUserToProject(int id, int userId)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        if (!isAdmin) return Forbid();

        bool result = await _projectService.AddUserToProjectAsync(id, userId);
        if (!result) return BadRequest("User already assigned to project");
        
        return Ok();
    }

    [HttpDelete("{id:int}/users/{userId:int}")]
    public async Task<ActionResult> RemoveUserFromProject(int id, int userId)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        if (!isAdmin) return Forbid();

        bool result = await _projectService.RemoveUserFromProjectAsync(id, userId);
        if (!result) return NotFound();
        
        return NoContent();
    }
}

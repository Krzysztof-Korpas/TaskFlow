using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models.Dto;

namespace TaskFlow.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TicketsApiController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IProjectService _projectService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketsApiController(ITicketService ticketService, IProjectService projectService, UserManager<ApplicationUser> userManager)
    {
        _ticketService = ticketService;
        _projectService = projectService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll([FromQuery] int? projectId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

        // If projectId is specified, check access
        if (projectId.HasValue)
        {
            var hasAccess = await _projectService.UserHasAccessToProjectAsync(projectId.Value, currentUser.Id, isAdmin);
            if (!hasAccess) return Forbid();
        }

        var list = await _ticketService.GetAllAsync(projectId);
        
        // Filter tickets based on project access
        if (!isAdmin)
        {
            var accessibleProjects = await _projectService.GetAllForUserAsync(currentUser.Id, false);
            var accessibleProjectIds = accessibleProjects.Select(p => p.Id).ToHashSet();
            list = list.Where(t => accessibleProjectIds.Contains(t.ProjectId));
        }

        return Ok(list.Select(t => t.ToDto()));
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<TicketDto>> GetByKey(string key)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var t = await _ticketService.GetByKeyAsync(key);
        if (t == null) return NotFound();

        var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        var hasAccess = await _projectService.UserHasAccessToProjectAsync(t.ProjectId, currentUser.Id, isAdmin);
        
        if (!hasAccess) return Forbid();

        return Ok(t.ToDto());
    }

    [HttpGet("id/{id:int}")]
    public async Task<ActionResult<TicketDto>> GetById(int id)
    {
        var t = await _ticketService.GetByIdAsync(id);
        if (t == null) return NotFound();
        return Ok(t.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest req)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        var hasAccess = await _projectService.UserHasAccessToProjectAsync(req.ProjectId, currentUser.Id, isAdmin);
        
        if (!hasAccess) return Forbid();

        var ticket = new Ticket
        {
            Title = req.Title,
            Description = req.Description,
            ProjectId = req.ProjectId,
            ReporterId = req.ReporterId,
            AssigneeId = req.AssigneeId,
            Type = req.Type,
            Priority = req.Priority
        };
        var created = await _ticketService.CreateAsync(ticket);
        return CreatedAtAction(nameof(GetByKey), new { key = created.Key }, created.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketDto>> Update(int id, [FromBody] UpdateTicketRequest req)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var ticket = await _ticketService.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        var hasAccess = await _projectService.UserHasAccessToProjectAsync(ticket.ProjectId, currentUser.Id, isAdmin);
        
        if (!hasAccess) return Forbid();

        var t = await _ticketService.UpdateAsync(id, req.Title, req.Description, req.Type, req.Status, req.Priority, req.AssigneeId);
        if (t == null) return NotFound();
        var full = await _ticketService.GetByIdAsync(id);
        return Ok((full ?? t).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        if (!await _ticketService.DeleteAsync(id)) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, [FromBody] AddCommentRequest req)
    {
        var ticket = await _ticketService.GetByIdAsync(id);
        if (ticket == null) return NotFound();
        var comment = await _ticketService.AddCommentAsync(id, 1, req.Body);
        return Ok(comment.ToDto());
    }
}

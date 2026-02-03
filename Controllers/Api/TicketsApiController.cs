using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models.Dto;

namespace TaskFlow.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TicketsApiController(ITicketService ticketService, IProjectService projectService, UserManager<ApplicationUser> userManager) : ControllerBase
{
    private readonly ITicketService _ticketService = ticketService;
    private readonly IProjectService _projectService = projectService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll([FromQuery] int? projectId)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

        // If projectId is specified, check access
        if (projectId.HasValue)
        {
            bool hasAccess = await _projectService.UserHasAccessToProjectAsync(projectId.Value, currentUser.Id, isAdmin);
            if (!hasAccess) return Forbid();
        }

        IEnumerable<Ticket> list = await _ticketService.GetAllAsync(projectId);
        
        // Filter tickets based on project access
        if (!isAdmin)
        {
            IEnumerable<Project> accessibleProjects = await _projectService.GetAllForUserAsync(currentUser.Id, false);
            HashSet<int> accessibleProjectIds = accessibleProjects.Select(p => p.Id).ToHashSet();
            list = list.Where(t => accessibleProjectIds.Contains(t.ProjectId));
        }

        return Ok(list.Select(t => t.ToDto()));
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<TicketDto>> GetByKey(string key)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        Ticket? t = await _ticketService.GetByKeyAsync(key);
        if (t == null) return NotFound();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool hasAccess = await _projectService.UserHasAccessToProjectAsync(t.ProjectId, currentUser.Id, isAdmin);
        
        if (!hasAccess) return Forbid();

        return Ok(t.ToDto());
    }

    [HttpGet("id/{id:int}")]
    public async Task<ActionResult<TicketDto>> GetById(int id)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        Ticket? t = await _ticketService.GetByIdAsync(id);
        if (t == null) return NotFound();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool hasAccess = await _projectService.UserHasAccessToProjectAsync(t.ProjectId, currentUser.Id, isAdmin);
        if (!hasAccess) return Forbid();

        return Ok(t.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest req)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool hasAccess = await _projectService.UserHasAccessToProjectAsync(req.ProjectId, currentUser.Id, isAdmin);
        
        if (!hasAccess) return Forbid();

        Ticket ticket = new Ticket
        {
            Title = req.Title,
            Description = req.Description,
            ProjectId = req.ProjectId,
            ReporterId = currentUser.Id,
            AssigneeId = req.AssigneeId,
            Type = req.Type,
            Priority = req.Priority
        };
        Ticket created = await _ticketService.CreateAsync(ticket);
        return CreatedAtAction(nameof(GetByKey), new { key = created.Key }, created.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketDto>> Update(int id, [FromBody] UpdateTicketRequest req)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        Ticket? ticket = await _ticketService.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool hasAccess = await _projectService.UserHasAccessToProjectAsync(ticket.ProjectId, currentUser.Id, isAdmin);
        
        if (!hasAccess) return Forbid();

        Ticket? t = await _ticketService.UpdateAsync(id, req.Title, req.Description, req.Type, req.Status, req.Priority, req.AssigneeId);
        if (t == null) return NotFound();
        Ticket? full = await _ticketService.GetByIdAsync(id);
        return Ok((full ?? t).ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        Ticket? ticket = await _ticketService.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool hasAccess = await _projectService.UserHasAccessToProjectAsync(ticket.ProjectId, currentUser.Id, isAdmin);
        if (!hasAccess) return Forbid();

        if (!await _ticketService.DeleteAsync(id)) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, [FromBody] AddCommentRequest req)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        Ticket? ticket = await _ticketService.GetByIdAsync(id);
        if (ticket == null) return NotFound();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool hasAccess = await _projectService.UserHasAccessToProjectAsync(ticket.ProjectId, currentUser.Id, isAdmin);
        if (!hasAccess) return Forbid();

        Comment comment = await _ticketService.AddCommentAsync(id, currentUser.Id, req.Body);
        return Ok(comment.ToDto());
    }
}

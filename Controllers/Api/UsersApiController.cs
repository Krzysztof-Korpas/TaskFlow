using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models.Dto;

namespace TaskFlow.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersApiController(IUserService userService, UserManager<ApplicationUser> userManager) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        if (!isAdmin) return Forbid();

        IEnumerable<ApplicationUser> list = await _userService.GetAllAsync();
        return Ok(list.Select(u => u.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        if (!isAdmin) return Forbid();

        ApplicationUser? u = await _userService.GetByIdAsync(id);
        if (u == null) return NotFound();
        return Ok(u.ToDto());
    }

    [HttpGet("is-admin")]
    public async Task<ActionResult<bool>> IsAdmin()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        
        bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        return Ok(isAdmin);
    }
}

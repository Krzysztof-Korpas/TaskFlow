using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Controllers;

[Authorize]
public class HomeController : Controller
{
    [Route("")]
    [Route("projects")]
    public IActionResult Index() => View();

    [Route("projects/{id:int}")]
    public IActionResult Project(int id) => View(id);

    [Route("projects/{id:int}/kanban")]
    public IActionResult Kanban(int id) => View(id);

    [Route("tickets/{key}")]
    public IActionResult Ticket(string key) => View((object)key);
}

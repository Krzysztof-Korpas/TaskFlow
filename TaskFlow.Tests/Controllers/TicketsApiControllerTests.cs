using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskFlow.Controllers.Api;
using TaskFlow.Models.Dto;
using TaskFlow.Services;
using Xunit;

namespace TaskFlow.Tests.Controllers;

public class TicketsApiControllerTests
{
    [Fact]
    public async Task AddComment_UsesCurrentUserAsAuthor()
    {
        ApplicationUser user = new ApplicationUser { Id = 7, Email = "user@test.local", UserName = "user@test.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = TestHelpers.CreateUserManagerMock();
        userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(false);

        Mock<ITicketService> ticketServiceMock = new Mock<ITicketService>();
        ticketServiceMock.Setup(s => s.GetByIdAsync(10))
            .ReturnsAsync(new Ticket { Id = 10, ProjectId = 2, ReporterId = 1, Title = "T" });
        ticketServiceMock.Setup(s => s.AddCommentAsync(10, user.Id, "hello"))
            .ReturnsAsync(new Comment { Id = 1, AuthorId = user.Id, TicketId = 10, Body = "hello" });

        Mock<IProjectService> projectServiceMock = new Mock<IProjectService>();
        projectServiceMock.Setup(s => s.UserHasAccessToProjectAsync(2, user.Id, false))
            .ReturnsAsync(true);

        TicketsApiController controller = new TicketsApiController(ticketServiceMock.Object, projectServiceMock.Object, userManagerMock.Object)
        {
            ControllerContext = TestHelpers.CreateControllerContext(user)
        };

        ActionResult<CommentDto> result = await controller.AddComment(10, new AddCommentRequest { Body = "hello" });

        Assert.IsType<OkObjectResult>(result.Result);
        ticketServiceMock.Verify(s => s.AddCommentAsync(10, user.Id, "hello"), Times.Once);
    }
}

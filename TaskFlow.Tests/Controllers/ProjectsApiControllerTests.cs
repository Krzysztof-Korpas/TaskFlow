using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskFlow.Controllers.Api;
using TaskFlow.Models.Dto;
using TaskFlow.Services;
using Xunit;

namespace TaskFlow.Tests.Controllers;

public class ProjectsApiControllerTests
{
    [Fact]
    public async Task Create_Forbids_WhenNotAdmin()
    {
        ApplicationUser user = new ApplicationUser { Id = 1, Email = "user@test.local", UserName = "user@test.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = TestHelpers.CreateUserManagerMock();
        userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(false);

        Mock<IProjectService> projectServiceMock = new Mock<IProjectService>();
        ProjectsApiController controller = new ProjectsApiController(projectServiceMock.Object, userManagerMock.Object)
        {
            ControllerContext = TestHelpers.CreateControllerContext(user)
        };

        ActionResult<ProjectDto> result = await controller.Create(new CreateProjectRequest { Key = "demo", Name = "Demo" });

        Assert.IsType<ForbidResult>(result.Result);
    }
}

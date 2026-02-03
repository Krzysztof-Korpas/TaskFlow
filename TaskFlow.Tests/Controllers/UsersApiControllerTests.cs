using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskFlow.Controllers.Api;
using TaskFlow.Services;
using Xunit;

namespace TaskFlow.Tests.Controllers;

public class UsersApiControllerTests
{
    [Fact]
    public async Task GetAll_Forbids_WhenNotAdmin()
    {
        ApplicationUser user = new ApplicationUser { Id = 1, Email = "user@test.local", UserName = "user@test.local" };
        Mock<UserManager<ApplicationUser>> userManagerMock = TestHelpers.CreateUserManagerMock();
        userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin"))
            .ReturnsAsync(false);

        Mock<IUserService> userServiceMock = new Mock<IUserService>();
        UsersApiController controller = new UsersApiController(userServiceMock.Object, userManagerMock.Object)
        {
            ControllerContext = TestHelpers.CreateControllerContext(user)
        };

        ActionResult<IEnumerable<UserDto>> result = await controller.GetAll();

        Assert.IsType<ForbidResult>(result.Result);
    }
}

using TaskFlow.Services;
using Xunit;

namespace TaskFlow.Tests.Services;

public class ProjectServiceTests
{
    [Fact]
    public async Task CreateAsync_UppercasesKey_AndRejectsDuplicate()
    {
        await using ApplicationDbContext db = TestHelpers.CreateDbContext();
        ProjectService service = new ProjectService(db);

        Project project = new Project { Key = "demo", Name = "Demo" };
        Project created = await service.CreateAsync(project);

        Assert.Equal("DEMO", created.Key);

        Project duplicate = new Project { Key = "DEMO", Name = "Other" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(duplicate));
    }
}

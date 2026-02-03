using TaskFlow.Services;
using Xunit;

namespace TaskFlow.Tests.Services;

public class TicketServiceTests
{
    [Fact]
    public async Task CreateAsync_SetsKeyAndCreatedAt()
    {
        await using ApplicationDbContext db = TestHelpers.CreateDbContext();
        Project project = new Project { Key = "DEMO", Name = "Demo" };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        TicketService service = new TicketService(db, new FakeRabbitMqService());
        Ticket ticket = new Ticket
        {
            Title = "Test",
            ProjectId = project.Id,
            ReporterId = 1
        };

        Ticket created = await service.CreateAsync(ticket);

        Assert.Equal($"DEMO-1", created.Key);
        Assert.True(created.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndTimestamp()
    {
        await using ApplicationDbContext db = TestHelpers.CreateDbContext();
        Project project = new Project { Key = "DEMO", Name = "Demo" };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        Ticket ticket = new Ticket
        {
            Title = "Old",
            Description = "Old",
            ProjectId = project.Id,
            ReporterId = 1
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        TicketService service = new TicketService(db, new FakeRabbitMqService());
        Ticket? updated = await service.UpdateAsync(ticket.Id, "New", "New", TicketType.Bug, TicketStatus.InProgress, TicketPriority.High, null);

        Assert.NotNull(updated);
        Assert.Equal("New", updated!.Title);
        Assert.Equal("New", updated.Description);
        Assert.Equal(TicketType.Bug, updated.Type);
        Assert.Equal(TicketStatus.InProgress, updated.Status);
        Assert.Equal(TicketPriority.High, updated.Priority);
        Assert.NotNull(updated.UpdatedAt);
    }

    private sealed class FakeRabbitMqService : IRabbitMqService
    {
        public void Publish(string exchange, string routingKey, string message) { }
        public void EnsureExchangeAndQueue(string exchange, string queue, string routingKey) { }
    }
}

namespace TaskFlow.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectStatus> ProjectStatuses => Set<ProjectStatus>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ProjectUserGroup> ProjectUserGroups => Set<ProjectUserGroup>();
    public DbSet<KanbanColumnPreference> KanbanColumnPreferences => Set<KanbanColumnPreference>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Users");

        builder.Entity<Ticket>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Ticket>()
            .HasOne(t => t.Status)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Ticket>()
            .HasOne(t => t.Reporter)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Comment>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProjectStatus>()
            .HasOne(s => s.Project)
            .WithMany(p => p.Statuses)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<KanbanColumnPreference>()
            .HasOne(p => p.Project)
            .WithMany(pr => pr.KanbanColumnPreferences)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<KanbanColumnPreference>()
            .HasOne(p => p.User)
            .WithMany(u => u.KanbanColumnPreferences)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<KanbanColumnPreference>()
            .HasOne(p => p.Status)
            .WithMany(s => s.ColumnPreferences)
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Project>().HasIndex(p => p.Key).IsUnique();
        builder.Entity<Ticket>().HasIndex(t => t.Key).IsUnique();
        builder.Entity<ProjectStatus>().HasIndex(s => new { s.ProjectId, s.Name }).IsUnique();
        builder.Entity<KanbanColumnPreference>()
            .HasIndex(p => new { p.ProjectId, p.UserId, p.StatusId })
            .IsUnique();

        builder.Entity<ProjectUserGroup>()
            .HasOne(pug => pug.Project)
            .WithMany(p => p.AssignedUsers)
            .HasForeignKey(pug => pug.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectUserGroup>()
            .HasOne(pug => pug.User)
            .WithMany(u => u.ProjectAssignments)
            .HasForeignKey(pug => pug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectUserGroup>()
            .HasIndex(pug => new { pug.ProjectId, pug.UserId })
            .IsUnique();
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager, IConfiguration config)
    {
        string? adminEmail = config["Seed:AdminEmail"];
        string? adminPassword = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("Seed admin credentials are missing. Provide Seed:AdminEmail and Seed:AdminPassword.");

        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        }

        ApplicationUser? admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true
            };
            IdentityResult result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        // Assign Admin role to admin user
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await db.Projects.AnyAsync()) return;

        Project project = new ()
        {
            Key = "DEMO",
            Name = "Projekt demonstracyjny",
            Description = "Przykładowy projekt do testów."
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        List<ProjectStatus> statuses =
        [
            new ProjectStatus { ProjectId = project.Id, Name = "Do zrobienia", SortOrder = 0, IsDefault = true },
            new ProjectStatus { ProjectId = project.Id, Name = "W toku", SortOrder = 1, IsDefault = false },
            new ProjectStatus { ProjectId = project.Id, Name = "Do przeglądu", SortOrder = 2, IsDefault = false },
            new ProjectStatus { ProjectId = project.Id, Name = "Zrobione", SortOrder = 3, IsDefault = false }
        ];
        db.ProjectStatuses.AddRange(statuses);
        await db.SaveChangesAsync();

        int defaultStatusId = statuses.First(s => s.IsDefault).Id;

        Ticket ticket = new ()
        {
            Key = "DEMO-1",
            Title = "Pierwsze zadanie",
            Description = "To jest przykładowe zadanie.",
            ProjectId = project.Id,
            ReporterId = admin.Id,
            StatusId = defaultStatusId,
            Priority = TicketPriority.Medium
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
    }
}

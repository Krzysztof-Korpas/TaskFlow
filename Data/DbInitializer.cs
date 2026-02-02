using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Data;

public static class DbInitializer
{
    private const string AdminEmail = "admin@stc.local";
    private const string AdminPassword = "Admin1!";

    public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        }

        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        // Assign Admin role to admin user
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await db.Projects.AnyAsync()) return;

        var project = new Project
        {
            Key = "DEMO",
            Name = "Projekt demonstracyjny",
            Description = "Przykładowy projekt do testów."
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var ticket = new Ticket
        {
            Key = "DEMO-1",
            Title = "Pierwsze zadanie",
            Description = "To jest przykładowe zadanie.",
            ProjectId = project.Id,
            ReporterId = admin.Id,
            Status = TicketStatus.ToDo,
            Priority = TicketPriority.Medium
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
    }
}

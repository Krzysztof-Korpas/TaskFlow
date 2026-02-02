using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TaskFlow.Data;
using TaskFlow.Localization;
using TaskFlow.Models;
using TaskFlow.Services;


namespace TaskFlow;

internal static class Program
{
    internal static async Task Main(string[] args)
    {

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            var conn = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=stc;Username=postgres;Password=postgres";
            options.UseNpgsql(conn);
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
        });

        builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
        builder.Services.AddScoped<ITicketService, TicketService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddLocalization();
        builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        builder.Services.AddControllersWithViews()
            .AddRazorRuntimeCompilation()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(type);
            });

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { new CultureInfo("pl"), new CultureInfo("en") };
            options.DefaultRequestCulture = new RequestCulture("pl");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            await db.Database.MigrateAsync();
            await DbInitializer.SeedAsync(db, userManager, roleManager);
        }

        var rabbit = app.Services.GetRequiredService<IRabbitMqService>();
        rabbit.EnsureExchangeAndQueue("stc.tickets", "stc.tickets.queue", "ticket.#");

        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();

        app.MapGet("/locales/{culture}.json", (string culture, IWebHostEnvironment env) =>
        {
            var path = Path.Combine(env.ContentRootPath, "Resources", $"{culture}.json");
            if (!File.Exists(path)) return Results.NotFound();
            return Results.Content(File.ReadAllText(path), "application/json");
        });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapControllers();

        app.Run();
    }
}
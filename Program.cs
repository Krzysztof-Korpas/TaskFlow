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
            string conn = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=stc;Username=postgres;Password=postgres";
            options.UseNpgsql(conn);
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
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
        builder.Services.AddScoped<IKanbanService, KanbanService>();

        builder.Services.AddLocalization();
        builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        IMvcBuilder mvcBuilder = builder.Services.AddControllersWithViews()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(type);
            });

        if (builder.Environment.IsDevelopment())
            mvcBuilder.AddRazorRuntimeCompilation();

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            CultureInfo[] supportedCultures = [new CultureInfo("pl"), new CultureInfo("en")];
            options.DefaultRequestCulture = new RequestCulture("pl");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        builder.Services.AddEndpointsApiExplorer();

        string[] corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (corsOrigins.Length == 0 && builder.Environment.IsDevelopment())
        {
            corsOrigins = ["http://localhost:5173", "http://localhost:3000"];
        }

        if (corsOrigins.Length > 0)
        {
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(corsOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                });
            });
        }

        WebApplication app = builder.Build();

        bool applyMigrations = builder.Configuration.GetValue<bool>("Database:ApplyMigrations");
        bool seedEnabled = builder.Configuration.GetValue<bool>("Seed:Enabled");

        using (IServiceScope scope = app.Services.CreateScope())
        {
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            RoleManager<IdentityRole<int>> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            if (applyMigrations || app.Environment.IsDevelopment())
                await db.Database.MigrateAsync();
            if (seedEnabled)
                await DbInitializer.SeedAsync(db, userManager, roleManager, builder.Configuration);
        }

        IRabbitMqService rabbit = app.Services.GetRequiredService<IRabbitMqService>();
        rabbit.EnsureExchangeAndQueue("stc.tickets", "stc.tickets.queue", "ticket.#");

        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        if (corsOrigins.Length > 0)
            app.UseCors();

        app.MapGet("/locales/{culture}.json", (string culture, IWebHostEnvironment env) =>
        {
            string path = Path.Combine(env.ContentRootPath, "Resources", $"{culture}.json");
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

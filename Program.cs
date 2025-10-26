using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.Data.SqlClient; // Додано для SqlException
using Polly; // Додано для Retry Policy

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DigitalLibrary.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

var app = builder.Build();

// --- Оновлений блок авто-міграції ---
try
{
    Console.WriteLine("[App Startup] Attempting to apply database migrations...");

    if (string.IsNullOrEmpty(connectionString))
    {
        Console.Error.WriteLine("[App Startup FATAL] Connection string 'DefaultConnection' is NULL or EMPTY.");
        throw new InvalidOperationException("Connection string 'DefaultConnection' is NULL or EMPTY.");
    }

    var retryPolicy = Policy
        .Handle<SqlException>()
        .WaitAndRetry(5, retryAttempt =>
        {
            var timeToWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            Console.Error.WriteLine($"[App Startup WARNING] Database connection failed (SqlException). Retrying in {timeToWait.TotalSeconds} seconds...");
            return timeToWait;
        });

    retryPolicy.Execute(() =>
    {
        Console.WriteLine("[App Startup] (RetryPolicy) Executing Database.Migrate()...");
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
        Console.WriteLine("[App Startup] (RetryPolicy) Database.Migrate() successful.");
    });

    Console.WriteLine("[App Startup] Database migrations applied successfully.");
}
catch (Exception ex)
{
    Console.Error.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    Console.Error.WriteLine("[App Startup FATAL ERROR] Failed to apply migrations after all retries.");
    Console.Error.WriteLine($"[App Startup FATAL ERROR] Exception: {ex.GetType().Name}");
    Console.Error.WriteLine($"[App Startup FATAL ERROR] Message: {ex.Message}");
    Console.Error.WriteLine(ex.ToString());
    Console.Error.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    throw;
}
// --- Кінець оновленого блоку ---


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
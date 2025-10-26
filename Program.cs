using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.Models;
using Microsoft.Data.SqlClient;
using Polly;

// Обгортаємо АБСОЛЮТНО ВСЕ в try-catch
try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- ПЕРЕВІРКА РЯДКА ПІДКЛЮЧЕННЯ ---
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        Console.Error.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Console.Error.WriteLine("[App Startup FATAL ERROR] Connection string 'DefaultConnection' is NULL or EMPTY.");
        Console.Error.WriteLine("CHECK YOUR 'ConnectionStrings__DefaultConnection' VARIABLE IN CLOUD RUN!");
        Console.Error.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        throw new InvalidOperationException("Connection string 'DefaultConnection' is NULL or EMPTY.");
    }
    // --- КІНЕЦЬ ПЕРЕВІРКИ ---

    builder.Services.AddControllersWithViews();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
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

    // --- Блок авто-міграції ---
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
    // --- Кінець блоку авто-міграції ---

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
}
catch (Exception ex)
{
    // Цей catch тепер "з-ловить" АБСОЛЮТНО ВСЕ
    Console.Error.WriteLine($"[App Startup UBER-FATAL ERROR] {ex.GetType().Name}: {ex.Message}");
    Console.Error.WriteLine(ex.ToString());
    throw;
}
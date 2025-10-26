using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DigitalLibrary.Data;
using DigitalLibrary.Models;
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<DigitalLibrary.Data.ApplicationDbContext>();

    int maxRetries = 10;
    int delayInSeconds = 5;

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
           
            logger.LogInformation($"Attempting to connect to database (Attempt {i + 1}/{maxRetries})...");
            dbContext.Database.Migrate();
            logger.LogInformation("Database migration successful!");
            break; 
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
          
            logger.LogWarning(ex, $"Database connection failed. Retrying in {delayInSeconds} seconds...");
            if (i == maxRetries - 1)
            {
                
                logger.LogError("Could not connect to database after all retries. Application is shutting down.");
                throw;
            }
           
            System.Threading.Thread.Sleep(delayInSeconds * 1000);
        }
    }
}

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

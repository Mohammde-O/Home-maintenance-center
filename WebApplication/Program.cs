using HomeRepairHub.Data;
using HomeRepairHub.Models;
using Microsoft.EntityFrameworkCore;


var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString != null && connectionString.Contains("../DB/"))
{
    var currentPath = builder.Environment.ContentRootPath;
    while (!string.IsNullOrEmpty(currentPath)) 
    {
        if (Directory.Exists(Path.Combine(currentPath, "DB")))
        {
            var dbPath = Path.Combine(currentPath, "DB", "HomeRepair.db");
            connectionString = $"Data Source={dbPath}";
            break;
        }
        currentPath = Directory.GetParent(currentPath)?.FullName;
    }
}




builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        
        if (!context.Users.Any())
        {
            context.Users.Add(new User 
            { 
                Id = Guid.NewGuid(),
                Name = "المسؤول",
                Email = "admin@example.com",
                Password = "admin",
                Role = "Admin"
            });
            context.SaveChanges();
        }

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();



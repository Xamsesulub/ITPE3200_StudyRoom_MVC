using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MVC.DAL;

public static class DBInit
{
    public static async Task SeedAsync(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync( new IdentityRole("Admin"));
    var admin = await userManager.FindByNameAsync("admin");
    if (admin == null)
    {
        admin = new IdentityUser { UserName = "admin" };
        var result = await userManager.CreateAsync(admin, "Test123?");
    }
    
    if (!await userManager.IsInRoleAsync(admin, "Admin"))
        await userManager.AddToRoleAsync(admin, "Admin");
    
}
}
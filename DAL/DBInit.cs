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

        if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));  // If the role Admin does not exist, create it.
        if (!await roleManager.RoleExistsAsync("Student")) await roleManager.CreateAsync(new IdentityRole("Student"));  // Same with student

        var admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new IdentityUser { UserName = "admin" };
            var result = await userManager.CreateAsync(admin, "Test123?");  // Creates an account, admin, with the Admin role
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");

        var bruker = await userManager.FindByNameAsync("bruker");

        if (bruker == null)
        {
            bruker = new IdentityUser { UserName = "bruker" };
            var result = await userManager.CreateAsync(bruker, "321Test.");  // Creates an account, bruker, with the Student role
        }

        if (!await userManager.IsInRoleAsync(bruker, "Student"))
            await userManager.AddToRoleAsync(bruker, "Student");

    }
}
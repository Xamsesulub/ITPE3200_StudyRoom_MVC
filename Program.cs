using Microsoft.EntityFrameworkCore;
using MVC.DAL;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Identity; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options=>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Connection to the repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

// Identity 
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // No email config for local testing
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
   options.LoginPath = "/Account/Login";
   options.LogoutPath = "/Account/LogOut";
   options.AccessDeniedPath = "/Account/Login"; 
});

builder.Services.AddSession(options =>
{
   options.IdleTimeout = TimeSpan.FromMinutes(30);
   options.Cookie.HttpOnly = true;
   options.Cookie.IsEssential = true; 
});


builder.Services.AddSerilog((services, loggerConfiguration) =>
{
   loggerConfiguration
   .MinimumLevel.Information()
   .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Overrides all of Microsoft events so its not too verbose
   .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information) // The previous command would not have told you that your application had finished building
   .WriteTo.Console()
   .WriteTo.File($"Logs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    
}
await DBInit.SeedAsync(app);

app.UseRouting();

app.MapStaticAssets();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();

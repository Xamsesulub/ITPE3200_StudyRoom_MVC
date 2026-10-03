using Microsoft.EntityFrameworkCore;
using MVC.DAL;
using MVC.Services;
using Serilog;
using Serilog.Events; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options=>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Makes the booking rules available to the booking controller.
builder.Services.AddScoped<BookingService>();

builder.Services.AddScoped<IRoomRepository, RoomRepository>();

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

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();

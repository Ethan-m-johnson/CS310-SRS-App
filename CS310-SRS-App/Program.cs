using CS310_SRS_App.Model;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Microsoft.IO; // Make sure you have this using statement


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RecyclableMemoryStreamManager>();


builder.Services.AddDbContext<CS310SRSDatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));


QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllersWithViews();



builder.Services.AddSession(options => //for session variables
{
    options.IdleTimeout = TimeSpan.FromMinutes(15); // You can set Time.
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Must be before UseRouting for sessions varables 

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<EmailSender>();  //use AddScoped for services that should be created once per client request (connection).
builder.Services.AddHttpClient<SMSSender>(); //use AddHttpClient for services that make HTTP requests, providing a way to configure and manage HttpClient instances.

//initialize entity db connection
builder.Services.AddDbContext<DatabaseConnectionEF>(
    x => x.UseSqlServer(builder.Configuration.GetConnectionString("myconnection"))
    );

//initialize session
builder.Services.AddSession(
    a => a.IdleTimeout = TimeSpan.FromMinutes(10)
    );

//Add payment link createor class
builder.Services.AddHttpClient<PaymentLink>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Website}/{action=Index}/{id?}");

app.Run();
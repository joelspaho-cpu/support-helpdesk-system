using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.Data;
using TicketSupportSystem.Models;
using TicketSupportSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddAuthentication("UserScheme").AddCookie("UserScheme", options => 
{options.LoginPath = "/UserView/Login";}).AddCookie("StaffScheme", options => {options.AccessDeniedPath = "/AccessDenied";
options.LoginPath = "/StaffView/Login";});
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IUserService, UserService>();



// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

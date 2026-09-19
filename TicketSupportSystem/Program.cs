using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.Data;
using TicketSupportSystem.Services;


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
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<CodeHashingServiceOptions>(builder.Configuration.GetSection("Verification"));
builder.Services.AddScoped<ICodeHashingService, CodeHashingService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();

// Persist Data Protection keys (used to sign auth cookies) when a path is configured,
// so users stay signed in across container rebuilds.

var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrEmpty(keysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("TicketSupportSystem");
}


// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

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

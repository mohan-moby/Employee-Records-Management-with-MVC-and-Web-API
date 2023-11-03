using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
// Configuration setup
builder.Configuration.AddJsonFile("appsettings.json");
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IConfiguration>(configuration); // Add IConfiguration as a service

// Add session services
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "ASPNETTestCookie";
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/EmployeeMVC/Login"; 
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=EmployeeMVC}/{action=Login}/{id?}");

app.Run();

using CodeIntern.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CodeIntern.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using CodeIntern.DataAccess.Repository.IRepository;
using CodeIntern.DataAccess.Repository;
using CodeIntern.Models;
using System.Configuration;
using CodeIntern.Areas.Identity.Pages.Account;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options=>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser,IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.Configure<IdentityOptions>(options =>options.SignIn.RequireConfirmedEmail=true);

builder.Services.AddRazorPages();
builder.Services.AddScoped<ICompanyRepository,CompanyRepository>();
builder.Services.AddScoped<IInternshipRepository, InternshipRepository>();
builder.Services.AddScoped<IInternApplicationRepository, InternApplicationRepository>();
builder.Services.AddScoped<ISavedInternRepository, SavedInternRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
builder.Services.AddScoped<IEmailSender,EmailSender>();
builder.Services.AddScoped<RegisterModel>();
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

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using StudentManagerWebApp.Models;
using StudentManagerWebApp.Services;
namespace StudentManagerWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<StudentDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            builder.Services.AddScoped<PasswordService>();
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".aspnet-keys")));

            builder.Services
                .AddAuthentication("StudentManagerCookie")
                .AddCookie("StudentManagerCookie", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
                options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            });

            // Add services to the container.
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Students", "AdminOnly");
                options.Conventions.AuthorizeFolder("/Courses", "AdminOnly");
                options.Conventions.AuthorizeFolder("/Teachers", "AdminOnly");
                options.Conventions.AuthorizeFolder("/StudentPortal", "StudentOnly");
                options.Conventions.AuthorizeFolder("/TeacherPortal", "TeacherOnly");
                options.Conventions.AuthorizePage("/Dashboard");
                options.Conventions.AllowAnonymousToPage("/Account/Login");
                options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            DatabaseSeeder.SeedAsync(app.Services).GetAwaiter().GetResult();

            app.Run();
        }
    }
}

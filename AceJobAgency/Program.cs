using Microsoft.EntityFrameworkCore;
using System;
using AceJobAgency.Data;
using Microsoft.AspNetCore.Antiforgery;
using System.Linq;

namespace AceJobAgency
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

   
            builder.Services.AddControllersWithViews(options =>
            {
            
                options.Filters.AddService<AntiforgeryTo404Filter>();
            });

            builder.Services.AddScoped<SessionAuthFilter>();
            builder.Services.AddScoped<AntiforgeryTo404Filter>();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(
                        builder.Configuration.GetConnectionString("DefaultConnection")
                    )
                );
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();


            app.Use(async (context, next) =>
            {
                var sensitivePaths = new[]
                {
                    "/Home/ChangePassword",
                    "/Account/ResetPassword",
                    "/Account/ChangePassword",
                    "/Account/ForgotPassword"
                };

                try
                {
                    await next();
                }
                catch (AntiforgeryValidationException)
                {
                    if (sensitivePaths.Any(sp =>
                        context.Request.Path.StartsWithSegments(sp, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Response.Clear();
                        context.Response.Redirect("/Error/404");
                        return;
                    }
                    throw;
                }

                if (context.Response.StatusCode == StatusCodes.Status400BadRequest &&
                    sensitivePaths.Any(sp =>
                        context.Request.Path.StartsWithSegments(sp, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Response.Clear();
                    context.Response.Redirect("/Error/404");
                }
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
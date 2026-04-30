using AdaPET.Controllers.Services;
using AdaPET.Models;
using AdaPET.Services;
using Microsoft.EntityFrameworkCore;

namespace AdaPET
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ========== كل الـ Services هنا (قبل builder.Build) ==========

            // 1. إضافة الـ Controllers والـ Views
            builder.Services.AddControllersWithViews();

            // 2. ربط المشروع بقاعدة البيانات
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 3. إضافة الـ Services الخاصة بالمشروع
            builder.Services.AddScoped<IAuthService, AuthService>();
            // builder.Services.AddScoped<IEmailSender, EmailSender>(); // لو في

            // 4. إضافة Session Services (للتسجيل والدخول)
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ========== انتهى الـ Services ==========

            var app = builder.Build();  // ✅ الآن كل الـ Services جاهزة

            // ========== الـ Middleware هنا (بعد builder.Build) ==========

            // استخدام Session (كـ Middleware مش Service)
            app.UseSession();  // ✅ دي صح - تبقى هنا بعد builder.Build

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Регистрация репозитория в DI контейнере
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Устанавливаем тайм-аут сессии
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Добавляем аутентификацию с использованием cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                        .AddCookie(options =>
                        {
                            options.LoginPath = "/Account/Login";
                            options.LogoutPath = "/Account/Logout";
                            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                            options.SlidingExpiration = true;
                        });

var app = builder.Build();

app.UseSession();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

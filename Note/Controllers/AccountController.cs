using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Note.Models;
using Note.Models.ViewModels;
using Note.Repository;
using System.Security.Claims;

namespace Note.Controllers;

public class AccountController : Controller
{
    private readonly UserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _userRepository = new UserRepository(configuration);
        _configuration = configuration;
    }

    // GET: /Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (await _userRepository.IsUserExistAsync(model.Username))
            {
                ModelState.AddModelError("", "Пользователь с таким именем уже существует.");
                return View(model);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var user = new User
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                Email = model.Email
            };

            var userId = await _userRepository.RegisterUserAsync(user);
            return RedirectToAction("Login");
        }
        return View(model);
    }

    // GET: /Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userRepository.AuthenticateUserAsync(model.Username, model.Password);
            if (user != null)
            {
                // Создание claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                // Создание ClaimsIdentity и установка в ClaimsPrincipal
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Авторизация пользователя с установкой cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
        }
        return View(model);
    }

    // GET: /Account/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // GET: /Account/Profile
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        var profileViewModel = new UserProfileViewModel
        {
            Username = user.Username,
            Email = user.Email
        };

        return View(profileViewModel);
    }

    // POST: /Account/Profile
    [HttpPost]
    public async Task<IActionResult> Profile(UserProfileViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(model.CurrentPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    ModelState.AddModelError("CurrentPassword", "Неверный текущий пароль.");
                    return View(model);
                }
            }

            user.Email = model.Email;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError("NewPassword", "Пароли не совпадают.");
                    return View(model);
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            await _userRepository.UpdateUserAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }

        return View(model);
    }

    // POST: /Account/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        // Удаление пользователя из базы данных
        await _userRepository.DeleteUserAsync(user.Id);

        // Выход из системы
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Перенаправление на страницу логина
        return RedirectToAction("Login", "Account");
    }
}

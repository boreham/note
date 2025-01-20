using Microsoft.AspNetCore.Mvc;
using Note.Models;
using Note.Models.ViewModels;
using Note.Repository;

namespace Note.Controllers;

public class AdminController : Controller
{
    private readonly UserRepository _userRepository;
    private readonly UserRoleRepository _userRoleRepository;

    public AdminController(IConfiguration configuration)
    {
        _userRepository = new UserRepository(configuration);
        _userRoleRepository = new UserRoleRepository(configuration.GetConnectionString("DefaultConnection"));
    }

    // GET: /Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return View(users);
    }

    // GET: /Admin/EditUser/{id}
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: /Admin/EditUser/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, User model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Обновляем информацию о пользователе
            user.Username = model.Username;
            user.Email = model.Email;

            await _userRepository.UpdateUserAsync(user);
            return RedirectToAction(nameof(Users));
        }

        return View(model);
    }

    // GET: /Admin/DeleteUser/{id}
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: /Admin/DeleteUser/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int id)
    {
        await _userRepository.DeleteUserAsync(id);
        return RedirectToAction(nameof(Users));
    }

    // GET: /Admin/CreateUser
    public IActionResult CreateUser()
    {
        return View();
    }

    // POST: /Admin/CreateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(User model)
    {
        if (ModelState.IsValid)
        {
            // Хешируем пароль
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("defaultPassword"); // Можно заменить на пользовательский ввод пароля

            model.PasswordHash = hashedPassword;
            model.CreatedAt = DateTime.UtcNow;

            await _userRepository.CreateUserAsync(model);
            return RedirectToAction(nameof(Users));
        }

        return View(model);
    }

    // GET: Admin/Roles
    public async Task<IActionResult> Roles()
    {
        var roles = await _userRoleRepository.GetAllRolesAsync();
        return View(roles);
    }

    // GET: Admin/AssignRole/{userId}
    public async Task<IActionResult> AssignRole(int userId)
    {
        var roles = await _userRoleRepository.GetAllRolesAsync();
        var userRoles = await _userRoleRepository.GetRolesByUserIdAsync(userId);

        var model = new AssignRoleViewModel
        {
            UserId = userId,
            Roles = roles,
            UserRoles = userRoles
        };

        return View(model);
    }

    // POST: Admin/AssignRole
    [HttpPost]
    public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
    {
        if (model.SelectedRoleIds != null)
        {
            foreach (var roleId in model.SelectedRoleIds)
            {
                await _userRoleRepository.AssignRoleToUserAsync(model.UserId, roleId);
            }
        }
        return RedirectToAction("Users");
    }

    // POST: Admin/RemoveRole/{userId}
    [HttpPost]
    public async Task<IActionResult> RemoveRole(int userId, int roleId)
    {
        await _userRoleRepository.RemoveRoleFromUserAsync(userId, roleId);
        return RedirectToAction("AssignRole", new { userId });
    }

    // GET: Admin/CreateRole
    public IActionResult CreateRole()
    {
        return View();
    }

    // POST: Admin/CreateRole
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (!string.IsNullOrEmpty(roleName))
        {
            await _userRoleRepository.CreateRoleAsync(roleName);
        }
        return RedirectToAction("Roles");
    }

    // GET: Admin/DeleteRole/{roleId}
    public async Task<IActionResult> DeleteRole(int roleId)
    {
        await _userRoleRepository.DeleteRoleAsync(roleId);
        return RedirectToAction("Roles");
    }
}


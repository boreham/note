using Microsoft.Data.SqlClient;
using Note.Models;
using System.Data;

namespace Note.Repository;

public class UserRoleRepository
{
    private readonly string _connectionString;

    public UserRoleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Получение всех ролей
    public async Task<List<Role>> GetAllRolesAsync()
    {
        var roles = new List<Role>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM Roles";
            var command = new SqlCommand(query, connection);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    roles.Add(new Role
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"]
                    });
                }
            }
        }

        return roles;
    }

    // Получение ролей пользователя
    public async Task<List<Role>> GetRolesByUserIdAsync(int userId)
    {
        var roles = new List<Role>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT r.Id, r.Name FROM Roles r " +
                        "JOIN UserRoles ur ON r.Id = ur.RoleId WHERE ur.UserId = @UserId";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    roles.Add(new Role
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"]
                    });
                }
            }
        }

        return roles;
    }

    // Добавление роли пользователю
    public async Task AssignRoleToUserAsync(int userId, int roleId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            await command.ExecuteNonQueryAsync();
        }
    }

    // Удаление роли у пользователя
    public async Task RemoveRoleFromUserAsync(int userId, int roleId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            await command.ExecuteNonQueryAsync();
        }
    }

    // Создание новой роли
    public async Task CreateRoleAsync(string roleName)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "INSERT INTO Roles (Name) VALUES (@Name)";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", roleName);

            await command.ExecuteNonQueryAsync();
        }
    }

    // Удаление роли
    public async Task DeleteRoleAsync(int roleId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM Roles WHERE Id = @RoleId";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@RoleId", roleId);

            await command.ExecuteNonQueryAsync();
        }
    }
}


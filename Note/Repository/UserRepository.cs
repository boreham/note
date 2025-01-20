using Microsoft.Data.SqlClient;
using Note.Models;

namespace Note.Repository;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<bool> IsUserExistAsync(string username)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            var result = await command.ExecuteScalarAsync();
            return (int)result > 0;
        }
    }

    public async Task<int> RegisterUserAsync(User user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "INSERT INTO Users (Username, PasswordHash, Email) OUTPUT INSERTED.Id VALUES (@Username, @PasswordHash, @Email)";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Email", user.Email);

            var userId = await command.ExecuteScalarAsync();
            return (int)userId;
        }
    }

    public async Task<User> AuthenticateUserAsync(string username, string passwordHash)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        Id = (int)reader["Id"],
                        Username = (string)reader["Username"],
                        Email = (string)reader["Email"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    };
                }
                return null;
            }
        }
    }

    // Проверка пользователя по email и паролю
    public async Task<User> ValidateUserAsync(string email, string password)
    {
        User user = null;

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Запрос для получения пользователя по email
            var command = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", connection);
            command.Parameters.AddWithValue("@Email", email);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var storedPasswordHash = reader["PasswordHash"].ToString();  // Получаем хэш пароля из базы данных

                    // Проверка пароля (например, с использованием bcrypt или другого метода хеширования)
                    if (VerifyPassword(password, storedPasswordHash))
                    {
                        user = new User
                        {
                            Id = (int)reader["Id"],
                            Username = reader["UserName"].ToString(),
                            Email = reader["Email"].ToString(),
                            PasswordHash = storedPasswordHash
                        };
                    }
                }
            }
        }

        return user;  // Возвращаем пользователя или null, если он не найден
    }

    // Метод для проверки пароля (например, с bcrypt)
    private bool VerifyPassword(string enteredPassword, string storedHash)
    {
        // Здесь можно использовать BCrypt, SHA256 или другой алгоритм для проверки пароля
        return enteredPassword == storedHash;  // Для примера просто сравниваем строки
    }

    // Получение пользователя по ID
    public async Task<User> GetUserByIdAsync(int userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM Users WHERE Id = @UserId";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        Id = (int)reader["Id"],
                        Username = (string)reader["Username"],
                        Email = (string)reader["Email"],
                        PasswordHash = (string)reader["PasswordHash"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    };
                }
                return null;
            }
        }
    }

    // Обновление данных пользователя
    public async Task UpdateUserAsync(User user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "UPDATE Users SET Email = @Email, PasswordHash = @PasswordHash WHERE Id = @Id";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Id", user.Id);

            await command.ExecuteNonQueryAsync();
        }
    }

    // Удаление пользователя из базы данных
    public async Task DeleteUserAsync(int userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM Users WHERE Id = @UserId";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            await command.ExecuteNonQueryAsync();
        }
    }

    // Получение всех пользователей
    public async Task<List<User>> GetAllUsersAsync()
    {
        var users = new List<User>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "SELECT * FROM Users";
            var command = new SqlCommand(query, connection);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = (int)reader["Id"],
                        Username = (string)reader["Username"],
                        Email = (string)reader["Email"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
            }
        }

        return users;
    }

    // Создание нового пользователя
    public async Task CreateUserAsync(User user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "INSERT INTO Users (Username, Email, PasswordHash, CreatedAt) VALUES (@Username, @Email, @PasswordHash, @CreatedAt)";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

            await command.ExecuteNonQueryAsync();
        }
    }
}


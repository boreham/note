using Microsoft.Data.SqlClient;
using Note.Models;

namespace Note.Repository;

public interface IProductRepository
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product> GetProductByIdAsync(int id);
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Получить все продукты
    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM Products", connection);
            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString(),
                    Price = (decimal)reader["Price"],
                    Description = reader["Description"].ToString()
                });
            }
        }

        return products;
    }

    // Получить продукт по Id
    public async Task<Product> GetProductByIdAsync(int id)
    {
        Product product = null;

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM Products WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                product = new Product
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString(),
                    Price = (decimal)reader["Price"],
                    Description = reader["Description"].ToString()
                };
            }
        }

        return product;
    }

    // Добавить продукт
    public async Task AddProductAsync(Product product)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("INSERT INTO Products (Name, Price, Description) VALUES (@Name, @Price, @Description)", connection);
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@Description", product.Description);

            await command.ExecuteNonQueryAsync();
        }
    }

    // Обновить продукт
    public async Task UpdateProductAsync(Product product)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("UPDATE Products SET Name = @Name, Price = @Price, Description = @Description WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@Description", product.Description);
            command.Parameters.AddWithValue("@Id", product.Id);

            await command.ExecuteNonQueryAsync();
        }
    }

    // Удалить продукт
    public async Task DeleteProductAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = new SqlCommand("DELETE FROM Products WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}

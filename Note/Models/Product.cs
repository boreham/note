using System.ComponentModel.DataAnnotations;

namespace Note.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
    public decimal Price { get; set; }

    [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters")]
    public string Description { get; set; }
}

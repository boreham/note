using System.ComponentModel.DataAnnotations;

namespace Note.Models.ViewModels;

public class RegisterViewModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}


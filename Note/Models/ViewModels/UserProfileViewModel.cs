using System.ComponentModel.DataAnnotations;

namespace Note.Models.ViewModels;

public class UserProfileViewModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; }
}


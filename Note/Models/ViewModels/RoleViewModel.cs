using System.ComponentModel.DataAnnotations;

namespace Note.Models.ViewModels;

public class RoleViewModel
{
    [Required(ErrorMessage = "Название роли обязательно.")]
    public string RoleName { get; set; }
}

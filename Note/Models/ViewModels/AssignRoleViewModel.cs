using System.Data;

namespace Note.Models.ViewModels;

public class AssignRoleViewModel
{
    public int UserId { get; set; }
    public List<Role> Roles { get; set; }
    public List<Role> UserRoles { get; set; }
    public List<int> SelectedRoleIds { get; set; }
}


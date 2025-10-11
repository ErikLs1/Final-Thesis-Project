using App.Domain.Enum;

namespace App.Domain.Identity;

public class Roles
{
    public Guid Id { get; set; }
    public RoleType Name { get; set; }
    public string Description { get; set; } = null!;

    public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
}
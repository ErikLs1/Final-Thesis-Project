using App.Domain.Enum;

namespace App.Domain.Identity;

public class Role
{
    public Guid Id { get; set; }
    public RoleType Name { get; set; }
    public string Description { get; set; } = null!;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
}
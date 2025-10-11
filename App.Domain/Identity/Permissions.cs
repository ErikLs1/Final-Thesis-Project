using App.Domain.Base;
using App.Domain.Enum;

namespace App.Domain.Identity;

// TODO: Maybe do not need for demo
public class Permissions : IDomainId
{
    public Guid Id { get; set; }
    public PermissionType Name { get; set; }
    public string Description { get; set; } = null!;

    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
}
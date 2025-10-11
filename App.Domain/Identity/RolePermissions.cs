using App.Domain.Base;

namespace App.Domain.Identity;

// TODO: Maybe do not need for demo
public class RolePermissions : IDomainId
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    
    public Role Role { get; set; } = null!;
    public Permissions Permission { get; set; } = null!;
}
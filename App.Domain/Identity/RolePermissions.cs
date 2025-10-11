namespace App.Domain.Identity;

public class RolePermissions
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    
    public Roles Role { get; set; } = null!;
    public Permissions Permission { get; set; } = null!;
}
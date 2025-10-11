using Microsoft.AspNetCore.Identity;

namespace App.Domain.Base.Identity;

public abstract class BaseRole<TUserRole> : BaseRole<Guid, TUserRole>
    where TUserRole : class //BaseUserRole<BaseUser<TUserRole>, BaseRole<TUserRole>>
{
}


public abstract class BaseRole<TKey, TUserRole> : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : class //BaseUserRole<TKey, BaseUser<TKey, TUserRole>, BaseRole<TKey, TUserRole>>
{
    public ICollection<TUserRole>? UserRoles { get; set; }
}
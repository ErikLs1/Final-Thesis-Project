using App.Domain.Base;
using App.Domain.Base.Identity;

namespace App.Domain.Identity;

public class UserRole :  BaseUserRole<User, Role>, IDomainId
{
}
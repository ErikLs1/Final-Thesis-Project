using App.Domain.Base;
using App.Domain.Base.Identity;

namespace App.Domain.Identity;

public class RefreshToken : BaseRefreshToken, IDomainUserId, IDomainId
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
using App.Domain.Common;

namespace App.Domain.Base.Identity;

public class BaseRefreshToken : BaseRefreshToken<Guid>
{
    
}

public class BaseRefreshToken<TKey>
    where TKey: IEquatable<TKey>
{
    public string RefreshToken { get; set; } = Guid.NewGuid().ToString();
    public DateTime Expiration { get; set; } = DateTime.UtcNow.AddDays(7);
    
    
    public string? PreviousRefreshToken { get; set; }
    public DateTime PreviousExpiration { get; set; } = DateTime.UtcNow.AddDays(7);
}
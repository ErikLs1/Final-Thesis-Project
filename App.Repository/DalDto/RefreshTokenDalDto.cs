using App.Domain.Base;

namespace App.Repository.DalDto;

public class RefreshTokenDalDto : IDomainId
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
    public string? PreviousRefreshToken { get; set; }
    public DateTime PreviousExpiration { get; set; }
}
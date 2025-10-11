namespace App.Domain.Base;

public interface IDomainUserId : IDomainUserId<Guid> 
{
}

public interface IDomainUserId<TKey>
    where TKey : IEquatable<TKey>
{
    TKey UserId { get; set; }
}
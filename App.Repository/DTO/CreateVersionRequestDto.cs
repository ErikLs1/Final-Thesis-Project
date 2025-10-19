namespace App.Repository.DTO;

public record CreateVersionRequestDto(
    Guid LanguageId,
    IReadOnlyCollection<Guid> ResourceKeyIds,
    IDictionary<Guid, string> Content,
    string CreatedBy
);
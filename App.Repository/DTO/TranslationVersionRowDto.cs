namespace App.Repository.DTO;

public record TranslationVersionRowDto(
    Guid ResourceKeyId,
    string ResourceKey,
    string FriendlyKey,
    string? Content,
    Guid? LanguageId,
    string LanguageTag,
    int? VersionNumber
);
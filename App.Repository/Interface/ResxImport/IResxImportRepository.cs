namespace App.Repository.Interface.ResxImport;

public interface IResxImportRepository
{ 
    Task ImportFirstTranslationVersionForLanguageAsync(
        Guid languageId,
        IReadOnlyDictionary<string, string> entries);
}
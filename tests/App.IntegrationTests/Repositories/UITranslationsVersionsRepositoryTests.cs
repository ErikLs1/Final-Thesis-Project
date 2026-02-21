using App.Domain;
using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.IntegrationTests.Infrastructure;
using App.Repository.DTO;
using App.Repository.Impl;
using Microsoft.EntityFrameworkCore;

namespace App.IntegrationTests.Repositories;

public class UITranslationsVersionsRepositoryTests
{
    [Fact]
    public async Task CreateNewVersionAsync_CreatesNextVersionInWaitingReviewState()
    {
        await using var session = await SqliteDbSession.CreateAsync();

        var languageId = Guid.NewGuid();
        var resourceKeyId = Guid.NewGuid();

        session.DbContext.Languages.Add(new Languages
        {
            Id = languageId,
            LanguageTag = "et-EE",
            LanguageName = "Estonian",
            IsDefaultLanguage = true
        });

        session.DbContext.UIResourceKeys.Add(new UIResourceKeys
        {
            Id = resourceKeyId,
            ResourceKey = "Navigation.Home",
            FriendlyKey = "Navigation_Home"
        });

        session.DbContext.UITranslationVersions.Add(new UITranslationVersions
        {
            Id = Guid.NewGuid(),
            LanguageId = languageId,
            ResourceKeyId = resourceKeyId,
            VersionNumber = 0,
            TranslationState = TranslationState.Published,
            Content = "Avaleht",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        });

        await session.DbContext.SaveChangesAsync();

        var repository = new UITranslationsVersionsRepository(session.DbContext);

        var changed = await repository.CreateNewVersionAsync(new CreateVersionRequestDto(
            languageId,
            [resourceKeyId],
            new Dictionary<Guid, string> { [resourceKeyId] = "Kodu" },
            "translator@mail.com"
        ));

        var latest = await session.DbContext.UITranslationVersions
            .Where(v => v.LanguageId == languageId && v.ResourceKeyId == resourceKeyId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstAsync();

        Assert.Equal(1, changed);
        Assert.Equal(1, latest.VersionNumber);
        Assert.Equal(TranslationState.WaitingReview, latest.TranslationState);
        Assert.Equal("Kodu", latest.Content);
    }
}

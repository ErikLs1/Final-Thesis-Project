using App.Domain;
using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.IntegrationTests.Infrastructure;
using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using App.Repository.Impl;
using App.Repository.Pager;
using Microsoft.EntityFrameworkCore;

namespace App.IntegrationTests.Repositories;

public class UITranslationRepositoryTests
{
    [Fact]
    public async Task UpdateTranslationStateAsync_UpdatesVersionState()
    {
        await using var session = await SqliteDbSession.CreateAsync();

        var languageId = Guid.NewGuid();
        var resourceKeyId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        session.DbContext.Languages.Add(new Languages
        {
            Id = languageId,
            LanguageTag = "en-US",
            LanguageName = "English",
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
            Id = versionId,
            LanguageId = languageId,
            ResourceKeyId = resourceKeyId,
            VersionNumber = 1,
            TranslationState = TranslationState.WaitingReview,
            Content = "Home",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        });

        await session.DbContext.SaveChangesAsync();

        var repository = new UITranslationRepository(session.DbContext);

        var changed = await repository.UpdateTranslationStateAsync(new UpdateTranslationStateRequestDto(
            versionId,
            TranslationState.Rejected,
            "reviewer@mail.com"
        ));

        var updated = await session.DbContext.UITranslationVersions.FirstAsync(v => v.Id == versionId);

        Assert.Equal(1, changed);
        Assert.Equal(TranslationState.Rejected, updated.TranslationState);
    }

    [Fact]
    public async Task GetFilteredUITranslationsAsync_FiltersByWaitingReviewState()
    {
        await using var session = await SqliteDbSession.CreateAsync();

        var languageId = Guid.NewGuid();
        var waitingKeyId = Guid.NewGuid();
        var approvedKeyId = Guid.NewGuid();

        session.DbContext.Languages.Add(new Languages
        {
            Id = languageId,
            LanguageTag = "et-EE",
            LanguageName = "Estonian",
            IsDefaultLanguage = true
        });

        session.DbContext.UIResourceKeys.AddRange(
            new UIResourceKeys
            {
                Id = waitingKeyId,
                ResourceKey = "Navigation.Home",
                FriendlyKey = "Navigation_Home"
            },
            new UIResourceKeys
            {
                Id = approvedKeyId,
                ResourceKey = "Navigation.About",
                FriendlyKey = "Navigation_About"
            }
        );

        session.DbContext.UITranslationVersions.AddRange(
            new UITranslationVersions
            {
                Id = Guid.NewGuid(),
                LanguageId = languageId,
                ResourceKeyId = waitingKeyId,
                VersionNumber = 2,
                TranslationState = TranslationState.WaitingReview,
                Content = "Kodu",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seed"
            },
            new UITranslationVersions
            {
                Id = Guid.NewGuid(),
                LanguageId = languageId,
                ResourceKeyId = approvedKeyId,
                VersionNumber = 3,
                TranslationState = TranslationState.Approved,
                Content = "Meist",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seed"
            }
        );

        await session.DbContext.SaveChangesAsync();

        var repository = new UITranslationRepository(session.DbContext);

        var result = await repository.GetFilteredUITranslationsAsync(
            new FilteredTranslationsRequestDto(languageId, null, TranslationState.WaitingReview),
            new PagedRequest { Page = 1, PageSize = 10 }
        );

        Assert.Single(result.Items);
        Assert.Equal("Navigation_Home", result.Items[0].FriendlyKey);
        Assert.Equal(TranslationState.WaitingReview, result.Items[0].TranslationState);
    }
}

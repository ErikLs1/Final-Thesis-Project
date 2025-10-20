using System.ComponentModel.DataAnnotations;
using App.Domain.Enum;

namespace WebApp.Models;

public class ChangeStateVm
{
    [Required]
    public Guid TranslationVersionId { get; set; }
    public string LanguageTag { get; set; } = default!;
    public string ResourceKey { get; set; } = default!;
    public string FriendlyKey { get; set; } = default!;
    public int VersionNumber { get; set; }
    public string Content { get; set; } = default!;
    public TranslationState CurrentState { get; set; }

    [Required]
    public TranslationState NewState { get; set; }
}
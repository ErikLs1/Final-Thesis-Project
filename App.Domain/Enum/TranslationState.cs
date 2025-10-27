namespace App.Domain.Enum;

public enum TranslationState
{
    Depricated, // Translation that is not used anymore
    Published, // Translation that is currently live
    Approved, // Translation that is approved to use by the reviewer
    Rejected, // Translation that was rejected by the reviewer
    WaitingReview // Newly created translation that is waiting review
}
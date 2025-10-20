namespace WebApp.Models;

public class CreateVersionsVm
{
    public Guid LanguageId { get; set; }
    public List<LanguageOption> Languages { get; set; } = new();
    public List<Item> Items { get; set; } = new();

    public class LanguageOption
    {
        public Guid Id { get; set; }
        public string Display { get; set; } = default!;
    }

    public class Item
    {
        public Guid ResourceKeyId { get; set; }
        public string ResourceKey { get; set; } = default!;
        public string FriendlyKey { get; set; } = default!;
        public string? DefaultContent { get; set; }
        public bool Include { get; set; }
        public string? Content { get; set; }
    }
}
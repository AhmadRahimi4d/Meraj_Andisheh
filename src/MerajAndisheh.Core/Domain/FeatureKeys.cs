namespace MerajAndisheh.Core.Domain;

/// <summary>کلیدهای ماژول‌های اختیاری (FeatureFlag، بخش ۵.۳ سند).</summary>
public static class FeatureKeys
{
    public const string Comments = "Comments";
    public const string ContentRequests = "ContentRequests";
    public const string Films = "Films";
    public const string Books = "Books";
    public const string Games = "Games";
    public const string StoryGenerator = "StoryGenerator";
    public const string NewsTicker = "NewsTicker";

    public static IReadOnlyList<string> All { get; } =
        [Comments, ContentRequests, Films, Books, Games, StoryGenerator, NewsTicker];
}

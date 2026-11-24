using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Core.Domain.Enums;
using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>مثال‌های بخش ۳.۳ سند: قواعد انتشار محتوا.</summary>
public class ContentPublishValidatorTests
{
    private readonly ContentPublishValidator _validator = new();

    private static ContentItem ValidArticle() => new()
    {
        Title = "مسیر اقتصاد",
        Slug = "مسیر-اقتصاد",
        Summary = "خلاصه‌ی کوتاه",
        Format = ContentFormat.Article,
        Body = "<p>متن مقاله</p>",
        ThumbnailKey = "thumbs/a.jpg",
        Topics = { new ContentTopic { TopicId = 1 } },
        Sources = { new ContentSource { Title = "منبع اول" } }
    };

    [Fact]
    public void Article_WithEverything_IsPublishable()
    {
        var result = _validator.Validate(ValidArticle());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Article_WithoutSource_IsRejectedWithSourceRequired()
    {
        var content = ValidArticle();
        content.Sources.Clear();

        var result = _validator.Validate(content);

        Assert.False(result.IsValid);
        Assert.True(result.Has(PublishError.SourceRequired));
    }

    [Fact]
    public void Source_WithBlankTitle_DoesNotCount()
    {
        var content = ValidArticle();
        content.Sources.Clear();
        content.Sources.Add(new ContentSource { Title = "   ", Url = "https://example.org" });

        Assert.True(_validator.Validate(content).Has(PublishError.SourceRequired));
    }

    [Fact]
    public void Video_WithoutFileAndLink_IsRejected()
    {
        var content = ValidArticle();
        content.Format = ContentFormat.Video;
        content.Body = null;

        var result = _validator.Validate(content);

        Assert.True(result.Has(PublishError.MainFileOrLinkRequired));
    }

    [Fact]
    public void Video_WithExternalLinkAndOneSource_IsPublishable()
    {
        var content = ValidArticle();
        content.Format = ContentFormat.Video;
        content.Body = null;
        content.Files.Add(new ContentFile
        {
            Kind = FileKind.Main,
            StorageType = StorageType.External,
            ExternalUrl = "https://www.aparat.com/v/abc",
            OriginalFileName = "aparat",
            MimeType = "text/uri-list"
        });

        Assert.True(_validator.Validate(content).IsValid);
    }

    [Fact]
    public void Article_WithoutBodyAndWithoutFile_IsRejectedWithArticleBodyRequired()
    {
        var content = ValidArticle();
        content.Body = "   ";

        var result = _validator.Validate(content);

        Assert.True(result.Has(PublishError.ArticleBodyRequired));
        Assert.False(result.Has(PublishError.MainFileOrLinkRequired));
    }

    [Fact]
    public void Content_WithoutTopic_IsRejected()
    {
        var content = ValidArticle();
        content.Topics.Clear();

        Assert.True(_validator.Validate(content).Has(PublishError.TopicRequired));
    }

    [Fact]
    public void Content_WithoutThumbnail_IsRejected()
    {
        var content = ValidArticle();
        content.ThumbnailKey = null;

        Assert.True(_validator.Validate(content).Has(PublishError.ThumbnailRequired));
    }

    [Fact]
    public void Content_WithoutFormat_IsRejected()
    {
        var content = ValidArticle();
        content.Format = default;

        Assert.True(_validator.Validate(content).Has(PublishError.FormatRequired));
    }

    [Fact]
    public void Title_Over200Chars_IsRejected_And_Exactly200_IsAccepted()
    {
        var tooLong = ValidArticle();
        tooLong.Title = new string('ا', ContentLimits.TitleMaxLength + 1);
        Assert.True(_validator.Validate(tooLong).Has(PublishError.TitleTooLong));

        var exact = ValidArticle();
        exact.Title = new string('ا', ContentLimits.TitleMaxLength);
        Assert.True(_validator.Validate(exact).IsValid);
    }

    [Fact]
    public void Summary_Missing_Or_Over600Chars_IsRejected()
    {
        var missing = ValidArticle();
        missing.Summary = "";
        Assert.True(_validator.Validate(missing).Has(PublishError.SummaryRequired));

        var tooLong = ValidArticle();
        tooLong.Summary = new string('ا', ContentLimits.SummaryMaxLength + 1);
        Assert.True(_validator.Validate(tooLong).Has(PublishError.SummaryTooLong));
    }

    [Fact]
    public void EmptyContent_ReportsAllMissingParts()
    {
        var result = _validator.Validate(new ContentItem());

        Assert.Contains(PublishError.TitleRequired, result.Errors);
        Assert.Contains(PublishError.SummaryRequired, result.Errors);
        Assert.Contains(PublishError.TopicRequired, result.Errors);
        Assert.Contains(PublishError.SourceRequired, result.Errors);
        Assert.Contains(PublishError.FormatRequired, result.Errors);
        Assert.Contains(PublishError.ThumbnailRequired, result.Errors);
    }
}

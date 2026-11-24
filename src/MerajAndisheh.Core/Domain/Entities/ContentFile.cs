using MerajAndisheh.Core.Domain.Enums;

namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>فایل محتوا: آپلودشده (Local یا S3) یا لینک خارجی (External).</summary>
public class ContentFile
{
    public int Id { get; set; }
    public int ContentId { get; set; }

    public FileKind Kind { get; set; } = FileKind.Main;
    public StorageType StorageType { get; set; } = StorageType.Local;

    /// <summary>مسیر نسبی یا کلید S3 (برای External خالی است).</summary>
    public string? StorageKey { get; set; }

    /// <summary>فقط برای External.</summary>
    public string? ExternalUrl { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long? SizeBytes { get; set; }
    public bool IsDownloadable { get; set; } = true;
    public int SortOrder { get; set; }
}

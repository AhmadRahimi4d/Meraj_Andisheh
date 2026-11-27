namespace MerajAndisheh.Core.Services;

/// <summary>یک بخش از منوی پنل. Permission = null و SuperAdminOnly = false یعنی برای هر همکار.</summary>
public sealed record AdminMenuItem(string Id, string LabelKey, string Path, string? Permission, bool SuperAdminOnly);

namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>موضوع درختی. ParentId = null یعنی موضوع ریشه.</summary>
public class Topic
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public Topic? Parent { get; set; }
    public List<Topic> Children { get; set; } = [];

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

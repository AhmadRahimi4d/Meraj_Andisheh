using MerajAndisheh.Core.Domain;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>کلیدهای دسترسی دقیقاً همان ده کلید بخش ۲ سند هستند.</summary>
public class PermissionKeysTests
{
    [Fact]
    public void AllKeys_MatchDesignDoc_InDisplayOrder()
    {
        Assert.Equal(
            new[]
            {
                "dashboard.view", "content.manage", "taxonomy.manage", "requests.manage", "comments.moderate",
                "users.manage", "layout.manage", "portals.manage", "settings.manage", "backup.manage"
            },
            PermissionKeys.All);
    }

    [Fact]
    public void Keys_AreUnique_AndCaseSensitiveLowercase()
    {
        Assert.Equal(PermissionKeys.All.Count, PermissionKeys.All.Distinct(StringComparer.Ordinal).Count());
        Assert.All(PermissionKeys.All, key => Assert.Equal(key.ToLowerInvariant(), key));
    }

    [Fact]
    public void SessionRules_MatchDesignDoc()
    {
        Assert.Equal(30, AccountRules.MemberSessionDays);
        Assert.Equal(8, AccountRules.StaffSessionHours);
        Assert.Equal(1, AccountRules.SessionValidationMinutes);
    }
}

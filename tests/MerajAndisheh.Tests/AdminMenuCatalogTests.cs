using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>همکار فقط بخش‌هایی را در منو می‌بیند که دسترسی دارد؛ همکاران فقط برای مدیر کل است.</summary>
public class AdminMenuCatalogTests
{
    private static string[] Visible(bool superAdmin, params string[] granted)
        => AdminMenuCatalog.VisibleFor(superAdmin, granted).Select(i => i.Id).ToArray();

    [Fact]
    public void SuperAdmin_SeesEverything()
    {
        Assert.Equal(new[] { "home", "staff", "users", "settings" }, Visible(true));
    }

    [Fact]
    public void Staff_WithoutKeys_SeesOnlyHome()
    {
        Assert.Equal(new[] { "home" }, Visible(false));
    }

    [Fact]
    public void Staff_SeesOnlyGrantedSections()
    {
        Assert.Equal(new[] { "home", "users" }, Visible(false, PermissionKeys.Users));
        Assert.Equal(new[] { "home", "settings" }, Visible(false, PermissionKeys.Settings));
        Assert.Equal(new[] { "home", "users", "settings" }, Visible(false, PermissionKeys.Users, PermissionKeys.Settings));
    }

    [Fact]
    public void Staff_NeverSeesStaffManagement_EvenWithEveryKey()
    {
        Assert.DoesNotContain("staff", Visible(false, PermissionKeys.All.ToArray()));
    }

    [Fact]
    public void KeysForSectionsNotBuiltYet_ShowNothingExtra()
    {
        Assert.Equal(new[] { "home" }, Visible(false, PermissionKeys.Content, PermissionKeys.Backup));
    }

    [Fact]
    public void EveryItem_UsesAKnownKey_AndPanelPaths()
    {
        foreach (var item in AdminMenuCatalog.Items)
        {
            Assert.True(item.Permission is null || PermissionRules.IsValidKey(item.Permission));
            Assert.True(AdminAccessRules.IsAdminPath(item.Path));
        }
    }
}

using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

public class PermissionRulesTests
{
    [Fact]
    public void SuperAdmin_HasEveryKey_WithoutAnyGrant()
    {
        foreach (var key in PermissionKeys.All)
            Assert.True(PermissionRules.HasPermission(true, false, [], key));
    }

    [Fact]
    public void Staff_HasOnlyTheKeysGrantedToThem()
    {
        string[] granted = [PermissionKeys.Users, PermissionKeys.Settings];

        Assert.True(PermissionRules.HasPermission(false, true, granted, PermissionKeys.Users));
        Assert.True(PermissionRules.HasPermission(false, true, granted, PermissionKeys.Settings));
        Assert.False(PermissionRules.HasPermission(false, true, granted, PermissionKeys.Content));
        Assert.False(PermissionRules.HasPermission(false, true, [], PermissionKeys.Users));
    }

    [Fact]
    public void Member_NeverHasAKey_EvenWithForgedClaims()
    {
        Assert.False(PermissionRules.HasPermission(false, false, PermissionKeys.All, PermissionKeys.Users));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Users.Manage")]   // حساس به بزرگی و کوچکی حروف
    [InlineData("content.delete")]
    public void UnknownKey_IsNeverGranted_NotEvenToSuperAdmin(string? key)
    {
        Assert.False(PermissionRules.HasPermission(true, true, PermissionKeys.All, key!));
    }

    [Fact]
    public void Normalize_DeduplicatesAndOrdersByDisplayOrder()
    {
        var result = PermissionRules.Normalize([PermissionKeys.Settings, PermissionKeys.Content, PermissionKeys.Settings]);

        Assert.Equal(new[] { PermissionKeys.Content, PermissionKeys.Settings }, result);
    }

    [Fact]
    public void Normalize_NullOrEmpty_GivesEmptyList()
    {
        Assert.Empty(PermissionRules.Normalize(null)!);
        Assert.Empty(PermissionRules.Normalize([])!);
    }

    [Fact]
    public void Normalize_UnknownKey_RejectsTheWholeSet()
    {
        Assert.Null(PermissionRules.Normalize([PermissionKeys.Content, "nonsense"]));
    }
}

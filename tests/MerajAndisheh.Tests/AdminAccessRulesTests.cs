using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>پنل مدیریت پنهان است (بخش ۳.۱۴): فقط همکار و مدیر کل، و بقیه ۴۰۴.</summary>
public class AdminAccessRulesTests
{
    [Theory]
    [InlineData("/admin", true)]
    [InlineData("/Admin", true)]
    [InlineData("/ADMIN", true)]
    [InlineData("/admin/", true)]
    [InlineData("/admin/users", true)]
    [InlineData("/Admin/Content/Edit", true)]
    [InlineData("/administrator", false)]
    [InlineData("/adminx", false)]
    [InlineData("/content/admin", false)]
    [InlineData("/account/login", false)]
    [InlineData("/", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsAdminPath_MatchesOnlyThePanel(string? path, bool expected)
    {
        Assert.Equal(expected, AdminAccessRules.IsAdminPath(path));
    }

    [Fact]
    public void OnlySuperAdminAndStaff_AreStaffRoles()
    {
        Assert.True(AdminAccessRules.IsStaffRole(RoleNames.SuperAdmin));
        Assert.True(AdminAccessRules.IsStaffRole(RoleNames.Staff));
        Assert.False(AdminAccessRules.IsStaffRole(RoleNames.Member));
        Assert.False(AdminAccessRules.IsStaffRole(null));
        Assert.False(AdminAccessRules.IsStaffRole("superadmin")); // نقش‌ها دقیقاً با همین نام‌اند
    }

    [Fact]
    public void HasStaffRole_ChecksAnyRole()
    {
        Assert.True(AdminAccessRules.HasStaffRole([RoleNames.Member, RoleNames.Staff]));
        Assert.False(AdminAccessRules.HasStaffRole([RoleNames.Member]));
        Assert.False(AdminAccessRules.HasStaffRole([]));
    }
}

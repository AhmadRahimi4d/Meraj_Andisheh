using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>مقصد بعد از ورود: عضو ← صفحه‌ی قبل یا اصلی؛ همکار و مدیر کل ← /admin.</summary>
public class LoginRedirectRulesTests
{
    [Fact]
    public void Member_GoesToReturnUrl_OrHome()
    {
        Assert.Equal("/content/x", LoginRedirectRules.ResolveTarget([RoleNames.Member], "/content/x"));
        Assert.Equal("/", LoginRedirectRules.ResolveTarget([RoleNames.Member], null));
        Assert.Equal("/", LoginRedirectRules.ResolveTarget([RoleNames.Member], ""));
    }

    [Fact]
    public void Staff_AndSuperAdmin_GoToAdmin_EvenWithReturnUrl()
    {
        Assert.Equal("/admin", LoginRedirectRules.ResolveTarget([RoleNames.Staff], null));
        Assert.Equal("/admin", LoginRedirectRules.ResolveTarget([RoleNames.SuperAdmin], "/content/x"));
        Assert.Equal("/admin", LoginRedirectRules.ResolveTarget([RoleNames.Member, RoleNames.Staff], null));
    }

    [Fact]
    public void UserWithoutRoles_IsTreatedAsMember()
    {
        Assert.Equal("/", LoginRedirectRules.ResolveTarget([], null));
    }
}

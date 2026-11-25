using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>معیار مرحله‌ی ۴: مدیر کل با رمز موقت تا رمز را عوض نکند به جای دیگر نمی‌رود.</summary>
public class PasswordChangeGateTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/content")]
    [InlineData("/admin")]
    [InlineData("/account/profile")]
    [InlineData(null)]
    public void MustChange_RedirectsEveryOtherPage(string? path)
    {
        Assert.True(PasswordChangeGate.ShouldRedirect(true, true, path));
    }

    [Theory]
    [InlineData("/account/change-password")]
    [InlineData("/Account/Change-Password")]
    [InlineData("/account/logout")]
    [InlineData("/css/tokens.css")]
    [InlineData("/js/ui.js")]
    [InlineData("/fonts/Vazirmatn-Variable.woff2")]
    [InlineData("/health")]
    public void MustChange_AllowsChangePasswordLogoutAndStaticFiles(string path)
    {
        Assert.False(PasswordChangeGate.ShouldRedirect(true, true, path));
    }

    [Fact]
    public void NoRedirect_WhenNotAuthenticated_OrNoChangeRequired()
    {
        Assert.False(PasswordChangeGate.ShouldRedirect(false, true, "/"));
        Assert.False(PasswordChangeGate.ShouldRedirect(true, false, "/"));
        Assert.False(PasswordChangeGate.ShouldRedirect(false, false, "/"));
    }

    [Fact]
    public void ChangePasswordPath_IsTheAllowedPath()
    {
        Assert.False(PasswordChangeGate.ShouldRedirect(true, true, PasswordChangeGate.ChangePasswordPath));
    }
}

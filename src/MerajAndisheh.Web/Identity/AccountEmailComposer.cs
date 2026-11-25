using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Identity;

/// <summary>ساخت و ارسال ایمیل‌های حساب (تأیید ایمیل و بازنشانی رمز). متن‌ها از resx می‌آید.</summary>
public sealed class AccountEmailComposer(IEmailSender sender, IStringLocalizer<SharedResource> L)
{
    public Task SendConfirmationAsync(AppUser user, string link, CancellationToken cancellationToken = default)
        => SendAsync(user, "Email.Confirm.Subject", "Email.Confirm.Body", link, cancellationToken);

    public Task SendPasswordResetAsync(AppUser user, string link, CancellationToken cancellationToken = default)
        => SendAsync(user, "Email.Reset.Subject", "Email.Reset.Body", link, cancellationToken);

    private Task SendAsync(AppUser user, string subjectKey, string bodyKey, string link, CancellationToken cancellationToken)
    {
        var subject = L[subjectKey].Value;
        var intro = Escape(L[bodyKey, user.DisplayName].Value);

        // لینک فقط نویسه‌های امن (ارقام، حروف لاتین، «-» و «_» و «&» و «=») دارد؛ برای ساده بودن کپی از لاگ، دستکاری نمی‌شود.
        var body = $"<p>{intro}</p><p><a href=\"{link}\">{link}</a></p>";

        return sender.SendAsync(user.Email!, subject, body, cancellationToken);
    }

    private static string Escape(string text)
        => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}

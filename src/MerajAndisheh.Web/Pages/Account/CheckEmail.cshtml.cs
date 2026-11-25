using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Pages.Account;

/// <summary>پیام «ایمیل خود را بررسی کن» برای ثبت‌نام، بازنشانی رمز و ارسال دوباره‌ی لینک تأیید.</summary>
public class CheckEmailModel : PageModel
{
    public string MessageKey { get; private set; } = "Account.CheckEmail.Register";

    public void OnGet(string? kind)
    {
        MessageKey = kind switch
        {
            "reset" => "Account.CheckEmail.Reset",
            "confirm" => "Account.CheckEmail.Confirm",
            _ => "Account.CheckEmail.Register"
        };
    }
}

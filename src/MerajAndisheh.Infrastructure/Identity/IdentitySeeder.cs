using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>
/// ساخت نقش‌ها و مدیر کل اولیه (بخش ۳.۱۵). رمز ثابت در کد نیست: ایمیل و رمز موقت از تنظیمات مخفی
/// (User Secrets در توسعه، تنظیمات سرور در تولید) می‌آید و در اولین ورود تغییر رمز اجباری است.
/// تکرارپذیر است: اگر مدیر کل وجود داشته باشد کاری نمی‌کند.
/// </summary>
public sealed class IdentitySeeder(
    RoleManager<AppRole> roles,
    UserManager<AppUser> users,
    IClock clock,
    ILogger<IdentitySeeder> logger)
{
    private const string DefaultSuperAdminDisplayName = "مدیر کل";

    public async Task SeedRolesAsync()
    {
        foreach (var name in RoleNames.All)
        {
            if (await roles.RoleExistsAsync(name)) continue;

            var result = await roles.CreateAsync(new AppRole(name));
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not create role '{name}': " +
                    string.Join(", ", result.Errors.Select(e => e.Code)));
        }
    }

    public async Task<SuperAdminSeedResult> SeedSuperAdminAsync(string? email, string? temporaryPassword)
    {
        await SeedRolesAsync();

        var existing = await users.GetUsersInRoleAsync(RoleNames.SuperAdmin);
        if (existing.Count > 0) return SuperAdminSeedResult.AlreadyExists;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(temporaryPassword))
        {
            logger.LogWarning("SuperAdmin is not configured (set SuperAdmin:Email and SuperAdmin:TemporaryPassword in secrets). Skipped.");
            return SuperAdminSeedResult.NotConfigured;
        }

        var user = new AppUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            DisplayName = DefaultSuperAdminDisplayName,
            MustChangePassword = true,
            IsActive = true,
            PersonalizationEnabled = true,
            CreatedAt = clock.UtcNow
        };

        var create = await users.CreateAsync(user, temporaryPassword);
        if (!create.Succeeded)
        {
            // فقط کد خطا لاگ می‌شود، هرگز رمز
            logger.LogError("Creating SuperAdmin failed: {Errors}", string.Join(", ", create.Errors.Select(e => e.Code)));
            return SuperAdminSeedResult.Failed;
        }

        var addRole = await users.AddToRoleAsync(user, RoleNames.SuperAdmin);
        if (!addRole.Succeeded)
        {
            await users.DeleteAsync(user);
            logger.LogError("Adding SuperAdmin role failed: {Errors}", string.Join(", ", addRole.Errors.Select(e => e.Code)));
            return SuperAdminSeedResult.Failed;
        }

        logger.LogInformation("SuperAdmin account created. Password change is required at first sign-in.");
        return SuperAdminSeedResult.Created;
    }
}

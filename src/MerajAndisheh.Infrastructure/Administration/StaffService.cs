using System.ComponentModel.DataAnnotations;
using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>
/// مدیریت همکاران (فقط مدیر کل؛ محافظت در لایه‌ی صفحه با PolicyNames.SuperAdminOnly): ساخت حساب با رمز موقت،
/// تعیین دسترسی‌های بخش‌به‌بخش، فعال/غیرفعال‌سازی. همکار حذف واقعی نمی‌شود (سازنده‌ی محتوا گم نشود).
/// </summary>
public sealed class StaffService(
    UserManager<AppUser> users,
    AppDbContext db,
    IClock clock,
    ITemporaryPasswordGenerator passwordGenerator,
    ILogger<StaffService> logger)
{
    public async Task<IReadOnlyList<StaffRow>> ListAsync(CancellationToken cancellationToken = default)
    {
        var staff = (await users.GetUsersInRoleAsync(RoleNames.Staff))
            .OrderBy(u => u.DisplayName, StringComparer.CurrentCulture)
            .ToList();

        var ids = staff.Select(u => u.Id).ToList();
        var permissions = await db.StaffPermissions
            .Where(p => ids.Contains(p.UserId))
            .ToListAsync(cancellationToken);

        return staff.Select(u => ToRow(u, permissions)).ToList();
    }

    /// <summary>یک همکار؛ اگر کاربر پیدا نشود یا همکار نباشد null.</summary>
    public async Task<StaffRow?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffAsync(id);
        if (user is null) return null;

        var permissions = await db.StaffPermissions.Where(p => p.UserId == id).ToListAsync(cancellationToken);
        return ToRow(user, permissions);
    }

    public async Task<StaffCreateResult> CreateAsync(CreateStaffRequest request, int actingUserId, CancellationToken cancellationToken = default)
    {
        var errors = new List<StaffError>();

        var email = request.Email?.Trim() ?? string.Empty;
        if (email.Length == 0 || !new EmailAddressAttribute().IsValid(email))
            errors.Add(StaffError.EmailInvalid);

        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        if (displayName.Length == 0 || displayName.Length > AccountRules.DisplayNameMaxLength)
            errors.Add(StaffError.DisplayNameInvalid);

        var permissions = PermissionRules.Normalize(request.Permissions);
        if (permissions is null)
            errors.Add(StaffError.PermissionInvalid);

        if (errors.Count > 0) return StaffCreateResult.Failure(errors);

        var temporaryPassword = passwordGenerator.Generate();
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true, // حساب را مدیر کل ساخته؛ نیازی به لینک تأیید نیست
            DisplayName = displayName,
            MustChangePassword = true, // اولین ورود: تغییر رمز اجباری
            PersonalizationEnabled = true,
            IsActive = true,
            CreatedAt = clock.UtcNow
        };

        var create = await users.CreateAsync(user, temporaryPassword);
        if (!create.Succeeded) return StaffCreateResult.Failure(MapErrors(create.Errors));

        var addRole = await users.AddToRoleAsync(user, RoleNames.Staff);
        if (!addRole.Succeeded)
        {
            await users.DeleteAsync(user); // حساب بی‌نقش نماند
            return StaffCreateResult.Failure([StaffError.Unknown]);
        }

        foreach (var key in permissions!)
        {
            db.StaffPermissions.Add(new StaffPermission { UserId = user.Id, PermissionKey = key });
        }
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Staff account {StaffId} created by user {ActorId} with permissions [{Permissions}]",
            user.Id, actingUserId, string.Join(", ", permissions));

        return StaffCreateResult.Success(user.Id, temporaryPassword);
    }

    /// <summary>
    /// کلیدهای دسترسی همکار را کامل جایگزین می‌کند. Security Stamp هم عوض می‌شود تا نشست باز او حداکثر
    /// ظرف AccountRules.SessionValidationMinutes دقیقه بسته شود و با ورود مجدد، دسترسی‌های تازه در کوکی بنشیند.
    /// </summary>
    public async Task<AdminOperationResult> UpdatePermissionsAsync(int staffId, IEnumerable<string>? keys, int actingUserId, CancellationToken cancellationToken = default)
    {
        var permissions = PermissionRules.Normalize(keys);
        if (permissions is null) return AdminOperationResult.Fail(AdminOperationError.PermissionInvalid);

        var user = await FindStaffAsync(staffId);
        if (user is null) return AdminOperationResult.Fail(AdminOperationError.NotFound);

        // فقط تفاوت اعمال می‌شود (حذف‌شده‌ها و تازه‌ها)؛ حذف و افزودنِ هم‌کلید در یک ذخیره، تداخل کلید در EF می‌سازد
        var current = await db.StaffPermissions.Where(p => p.UserId == staffId).ToListAsync(cancellationToken);
        var currentKeys = current.Select(p => p.PermissionKey).ToHashSet(StringComparer.Ordinal);

        db.StaffPermissions.RemoveRange(current.Where(p => !permissions.Contains(p.PermissionKey)));
        foreach (var key in permissions.Where(key => !currentKeys.Contains(key)))
        {
            db.StaffPermissions.Add(new StaffPermission { UserId = staffId, PermissionKey = key });
        }
        await db.SaveChangesAsync(cancellationToken);

        var stamp = await users.UpdateSecurityStampAsync(user);
        if (!stamp.Succeeded) return AdminOperationResult.Fail(AdminOperationError.Unknown);

        logger.LogInformation("Staff {StaffId} permissions changed by user {ActorId} to [{Permissions}]",
            staffId, actingUserId, string.Join(", ", permissions));

        return AdminOperationResult.Ok;
    }

    public async Task<AdminOperationResult> SetActiveAsync(int staffId, bool active, int actingUserId, CancellationToken cancellationToken = default)
    {
        var user = await FindStaffAsync(staffId);
        if (user is null) return AdminOperationResult.Fail(AdminOperationError.NotFound);

        if (!await AccountActivation.ApplyAsync(users, user, active))
            return AdminOperationResult.Fail(AdminOperationError.Unknown);

        logger.LogInformation("Staff {StaffId} {State} by user {ActorId}", staffId, active ? "activated" : "deactivated", actingUserId);
        return AdminOperationResult.Ok;
    }

    /// <summary>فقط حساب‌هایی که نقش همکار دارند (مدیر کل و عضو را نمی‌گیرد).</summary>
    private async Task<AppUser?> FindStaffAsync(int id)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null) return null;

        return await users.IsInRoleAsync(user, RoleNames.Staff) ? user : null;
    }

    private static StaffRow ToRow(AppUser user, IEnumerable<StaffPermission> allPermissions)
    {
        var granted = allPermissions.Where(p => p.UserId == user.Id).Select(p => p.PermissionKey).ToHashSet(StringComparer.Ordinal);
        var ordered = PermissionKeys.All.Where(granted.Contains).ToList(); // به ترتیب ثابت نمایش

        return new StaffRow(user.Id, user.DisplayName, user.Email, user.IsActive, user.MustChangePassword,
            user.CreatedAt, user.LastLoginAt, ordered);
    }

    private static List<StaffError> MapErrors(IEnumerable<IdentityError> identityErrors)
    {
        var mapped = new List<StaffError>();
        foreach (var error in identityErrors)
        {
            var item = error.Code switch
            {
                "DuplicateEmail" or "DuplicateUserName" => StaffError.EmailTaken,
                "InvalidEmail" or "InvalidUserName" => StaffError.EmailInvalid,
                _ => StaffError.Unknown
            };
            if (!mapped.Contains(item)) mapped.Add(item);
        }
        return mapped;
    }
}

using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Infrastructure.Administration;

/// <summary>
/// صفحه‌ی کاربران (کلید users.manage): فهرست و جستجوی اعضا و فعال/غیرفعال‌سازی. فقط اعضای عادی؛
/// همکاران و مدیر کل اینجا دیده یا تغییر داده نمی‌شوند (آن‌ها فقط از صفحه‌ی همکاران و فقط توسط مدیر کل).
/// </summary>
public sealed class MemberAdminService(
    UserManager<AppUser> users,
    AppDbContext db,
    ILogger<MemberAdminService> logger)
{
    private const int MaxPageSize = 100;

    /// <param name="query">قسمتی از نام نمایشی، ایمیل یا شماره‌ی موبایل (ارقام فارسی هم پذیرفته می‌شود).</param>
    public async Task<PagedResult<MemberRow>> SearchAsync(string? query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var memberRoleId = await db.Roles
            .Where(r => r.Name == RoleNames.Member)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (memberRoleId == 0) return new PagedResult<MemberRow>([], page, pageSize, 0);

        var members = db.Users.Where(u => db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == memberRoleId));

        var term = DigitNormalizer.ToLatin(query).Trim();
        if (term.Length > 0)
        {
            members = members.Where(u =>
                u.DisplayName.Contains(term) ||
                (u.Email != null && u.Email.Contains(term)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        var total = await members.CountAsync(cancellationToken);

        var items = await members
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new MemberRow(u.Id, u.DisplayName, u.Email, u.PhoneNumber, u.IsActive, u.CreatedAt, u.LastLoginAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<MemberRow>(items, page, pageSize, total);
    }

    public async Task<AdminOperationResult> SetActiveAsync(int memberId, bool active, int actingUserId, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(memberId.ToString());
        if (user is null) return AdminOperationResult.Fail(AdminOperationError.NotFound);

        // فقط عضو عادی؛ همکار و مدیر کل از این مسیر تغییر نمی‌کنند
        if (!await users.IsInRoleAsync(user, RoleNames.Member)
            || await users.IsInRoleAsync(user, RoleNames.Staff)
            || await users.IsInRoleAsync(user, RoleNames.SuperAdmin))
        {
            return AdminOperationResult.Fail(AdminOperationError.NotAllowed);
        }

        if (!await AccountActivation.ApplyAsync(users, user, active))
            return AdminOperationResult.Fail(AdminOperationError.Unknown);

        logger.LogInformation("Member {MemberId} {State} by user {ActorId}", memberId, active ? "activated" : "deactivated", actingUserId);
        return AdminOperationResult.Ok;
    }
}

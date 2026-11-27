using Microsoft.AspNetCore.Authorization;

namespace MerajAndisheh.Infrastructure.Authorization;

/// <summary>نیازمندی «داشتن یک کلید دسترسی» (مثلاً users.manage).</summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

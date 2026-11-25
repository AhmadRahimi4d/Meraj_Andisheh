using Microsoft.AspNetCore.Identity;

namespace MerajAndisheh.Infrastructure.Identity;

public class AppRole : IdentityRole<int>
{
    public AppRole()
    {
    }

    public AppRole(string roleName) : base(roleName)
    {
    }
}

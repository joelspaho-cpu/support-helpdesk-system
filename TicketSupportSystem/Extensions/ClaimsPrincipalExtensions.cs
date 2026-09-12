using System.Security.Claims;

namespace TicketSupportSystem.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetId(this ClaimsPrincipal user)
    {
        if (user == null)
        {
            return null;
        }
        return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;
    }
}
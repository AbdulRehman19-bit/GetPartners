using System.Security.Claims;

namespace GetPartners.Web.Services;

public static class TokenHelper
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        string? value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(value!);
    }
}
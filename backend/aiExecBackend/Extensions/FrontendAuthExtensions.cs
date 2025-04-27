using Microsoft.AspNetCore.Identity;
using Models;

namespace aiExecBackend.Extensions;

public static class FrontendAuthExtensions
{
    private static CookieOptions GetFrontendCookieOptions(TimeSpan? expireTimeSpan = null) => new()
    {
        HttpOnly = false,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = expireTimeSpan.HasValue 
            ? DateTime.UtcNow.Add(expireTimeSpan.Value) 
            : DateTime.UtcNow.AddDays(14) // Default to 14 days if no expiry specified
    };

    public static void SetFrontendAuthCookies(
        this SignInManager<UserInfo> signInManager, 
        string userId,
        TimeSpan? expireTimeSpan = null)
    {
        signInManager.Context.Response.Cookies.Append(
            "user_id", 
            userId, 
            GetFrontendCookieOptions(expireTimeSpan)
        );
    }
    public static void RemoveFrontendAuthCookies<TUser>(this SignInManager<TUser> signInManager)
        where TUser : class
    {
        signInManager.Context.Response.Cookies.Delete("user_id");
    }
}
using System.Security.Claims;
using aiExecBackend.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;

namespace aiExecBackend.Endpoints;

public static class OAuthEndpoints
{
    public static async Task GoogleFromFrontendHandler(HttpContext context, IWebHostEnvironment environment)
    {
        await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = environment.IsDevelopment() ? "/googleCallback" : "/api/googleCallback",
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme },
                    { "returnUrl", context.Request.Query["returnUrl"].ToString() }
                }
            });
    }

    public static async Task<IResult> GoogleCallbackHandler(
        HttpContext context,
        UserManager<UserInfo> userManager,
        SignInManager<UserInfo> signInManager)
    {
        var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded) return Results.Unauthorized();

        // Get the email from Google claims
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return Results.BadRequest("Email not provided by Google");

        // Check if user exists
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Create new user if they don't exist
            user = new UserInfo
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true // Since it's verified by Google
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return Results.BadRequest("Failed to create user");
            }

            // Optionally add external login info
            await userManager.AddLoginAsync(user, new UserLoginInfo(
                "Google",
                result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!,
                "Google"));
        }

        // Sign in the user
        await signInManager.SignInAsync(user, isPersistent: true);

        signInManager.SetFrontendAuthCookies(user.Id);

        // Redirect to the original redirect URI or home
        context.Response.Redirect(result.Properties?.RedirectUri ?? "/");
        return Results.Empty;
    }
}
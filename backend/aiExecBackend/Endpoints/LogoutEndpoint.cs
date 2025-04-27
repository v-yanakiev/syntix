using aiExecBackend.Extensions;
using Microsoft.AspNetCore.Identity;
using Models;

namespace aiExecBackend.Endpoints
{
    public static class LogoutEndpoint
    {
        public static async Task<IResult> Handler(SignInManager<UserInfo> signInManager)
        {
            await signInManager.SignOutAsync();
            signInManager.RemoveFrontendAuthCookies();

            return Results.Ok();
        }

    }
}

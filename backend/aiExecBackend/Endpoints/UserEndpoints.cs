using Microsoft.AspNetCore.Identity;
using Models;
using System.Security.Claims;
using aiExecBackend.Extensions;
using Stripe;

namespace aiExecBackend.Endpoints
{
    public static class UserEndpoints
    {
        public static Dictionary<string, string> GetClaimsHandler(ClaimsPrincipal user) =>
            user.Claims.ToDictionary(x => x.Type, x => x.Value);
        public static async Task<IResult> DeleteUserHandler(
            SignInManager<UserInfo> signInManager,
            UserManager<UserInfo> userManager)
        {
            var user = await signInManager.UserManager.GetUserAsync(signInManager.Context.User);

            if (user == null)
            {
                return Results.NotFound("User not found");
            }

            var deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return Results.Problem("Failed to delete user: " +
                    string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
            }

            await signInManager.SignOutAsync();

            //here we would insert the Fly Machine shutdown/deletion, etc.
            
            signInManager.RemoveFrontendAuthCookies();

            return Results.Ok();

        }
        public static async Task<IResult> UnsubscribeUserHandler(
            SignInManager<UserInfo> signInManager,
            PostgresContext postgresContext)
        {
            var user = await signInManager.UserManager.GetUserAsync(signInManager.Context.User);

            if (user == null)
            {
                return Results.NotFound("User not found");
            }

            var service = new SubscriptionService();
            _ = await service.CancelAsync(user.SubscriptionId);

            user.SetHasUnsubscribed();

            await postgresContext.SaveChangesAsync();
            
            return Results.Ok();

        }

        public static async Task<IResult> GetStateHandler(
            SignInManager<UserInfo> signInManager,
            PostgresContext postgresContext)
        {
            var user = await signInManager.UserManager.GetUserAsync(signInManager.Context.User);

            if (user == null)
            {
                return Results.NotFound("User not found");
            }

            if (user.HasUnsubscribed())
            {
                //TODO: add logic allowing a person to resubscribe after a month has passed
                return Results.Ok(new StateResponse("unsubscribed"));
            }

            if (user.IsInValidTrial())
            {
                return Results.Ok(new StateResponse("trial"));
            }

            return Results.Ok(user.Balance > 0 ? new StateResponse("paying") : new StateResponse("out"));
        }

        private record StateResponse(string State);
    }
}

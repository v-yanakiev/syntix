using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;

namespace aiExecBackend.Extensions
{
    public static class CustomAuthExtensions
    {
        public static async Task<TUser?> GetUserWithExecutedExpression<TUser>(
            this SignInManager<TUser> signInManager,
            Func<IQueryable<TUser>, IQueryable<TUser>>? expression = null)
            where TUser : IdentityUser
        {
            var userId = signInManager.UserManager.GetUserId(signInManager.Context.User);
            if (userId == null)
            {
                return null;
            }

            var dbContext = signInManager.Context.RequestServices
                .GetRequiredService<PostgresContext>();

            var userQuery = dbContext.Set<TUser>().AsQueryable()!;
            userQuery = expression == null ? userQuery : expression(userQuery);
            
            var user=await userQuery.FirstOrDefaultAsync(u => u.Id == userId);
            return user;
        }
    }
}

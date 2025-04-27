using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth
{
    public static class AuthorizationPolicies
    {
        public const string ActiveSubscription = "ActiveSubscription";

        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(ActiveSubscription, policy =>
                    policy.Requirements.Add(new ActiveSubscriptionRequirement()));
            });
        }
    }
}

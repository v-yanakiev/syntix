using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Interfaces
{
    public interface ISubscriptionService
    {
        Task<bool> HasActiveSubscriptionAsync(string userId);
    }
}

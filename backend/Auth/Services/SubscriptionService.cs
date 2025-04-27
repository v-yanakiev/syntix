using Auth.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Services
{
    public class SubscriptionService:ISubscriptionService
    {
        private readonly IConfiguration _configuration;

        public SubscriptionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> HasActiveSubscriptionAsync(string userId)
        {
            //using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            //await connection.OpenAsync();

            //var query = "SELECT EXISTS(SELECT 1 FROM subscriptions WHERE user_id = @UserId AND is_active = true)";
            //using var command = new NpgsqlCommand(query, connection);
            //command.Parameters.AddWithValue("UserId", userId);

            //return (bool)await command.ExecuteScalarAsync();
            return false;
        }
    }
}

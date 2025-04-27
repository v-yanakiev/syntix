using Npgsql;

namespace Models.Extensions;

public static class ConnectionStringExtensions
{
    public static string NormalizeConnectionString(this string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        // If it's already in Npgsql format, return as-is
        if (!connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)) return connectionString;

        // Convert URI format to Npgsql format
        var uri = new Uri(connectionString);
        var splitUserInfo = uri.UserInfo.Split(':');
        var userName = splitUserInfo.Length > 0 ? splitUserInfo[0] : null;
        var password = splitUserInfo.Length > 1 ? splitUserInfo[1] : null;
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = userName,
            Password = password,
            SslMode = SslMode.Require
        };

        return builder.ToString();
    }
}
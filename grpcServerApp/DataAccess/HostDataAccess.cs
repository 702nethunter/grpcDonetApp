using System;
using System.Threading.Tasks;
using Npgsql;

public class HostDataAccess
{
    private readonly string _connectionString;
    private readonly ILogger<HostDataAccess> _logger;

    public HostDataAccess(ILogger<HostDataAccess> logger, string connectionString)
    {
        _logger = logger;
        _connectionString = connectionString;
    }

    public async Task UpsertClientHostDataAsync(
        long hostId,
        string hostName,
        string hostIp,
        string clientOs,
        string clientVersion)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL upsert_client_host_data(@hostId, @hostName, @hostIp, @clientOs, @clientVersion)", conn);

        cmd.Parameters.AddWithValue("hostId", hostId);
        cmd.Parameters.AddWithValue("hostName", hostName ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("hostIp", hostIp ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("clientOs", clientOs ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("clientVersion", clientVersion ?? (object)DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }
}

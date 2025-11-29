using System.Data.SqlClient;

namespace VoteOp.AuthApi.Data;

public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration config)
    {
        _connectionString = config["SqlConnectionString"]
            ?? Environment.GetEnvironmentVariable("SqlConnectionString")
            ?? throw new InvalidOperationException("SqlConnectionString not configured.");
    }

    public SqlConnection Create()
    {
        return new SqlConnection(_connectionString);
    }
}
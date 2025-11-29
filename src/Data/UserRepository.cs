using Microsoft.Data.SqlClient;
using VoteOp.AuthApi.Models;

namespace VoteOp.AuthApi.Data;

public class UserRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public UserRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        await using var conn = _connectionFactory.Create();
        await conn.OpenAsync();

        const string sql = @"
            SELECT TOP 1 Id, Email, PasswordHash, PasswordSalt, CreatedUtc, FirstName, LastName
            FROM Users
            WHERE Email = @Email";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", email);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new UserEntity
        {
            Id = reader.GetGuid(0),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            PasswordSalt = reader.GetString(3),
            CreatedUtc = reader.GetDateTime(4),
            FirstName = reader.IsDBNull(5) ? null : reader.GetString(5),
            LastName = reader.IsDBNull(6) ? null : reader.GetString(6)
        };
    }

    public async Task<Guid> CreateAsync(UserEntity user)
    {
        await using var conn = _connectionFactory.Create();
        await conn.OpenAsync();

        const string sql = @"
            INSERT INTO Users (Id, Email, PasswordHash, PasswordSalt, CreatedUtc, FirstName, LastName)
            VALUES (@Id, @Email, @PasswordHash, @PasswordSalt, @CreatedUtc, @FirstName, @LastName);";

        user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
        user.CreatedUtc = DateTime.UtcNow;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", user.Id);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
        cmd.Parameters.AddWithValue("@CreatedUtc", user.CreatedUtc);
        cmd.Parameters.AddWithValue("@FirstName", (object?)user.FirstName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LastName", (object?)user.LastName ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
        return user.Id;
    }
}
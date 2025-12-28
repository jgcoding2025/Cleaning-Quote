using System.Security.Cryptography;

namespace CleaningQuote.Api.Data;

public sealed class TokenRepository
{
    private readonly ApiDb _db;

    public TokenRepository(ApiDb db)
    {
        _db = db;
    }

    public (string token, DateTime expiresAtUtc) CreateToken(Guid userId, TimeSpan lifetime)
    {
        var tokenId = Guid.NewGuid();
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes);
        var tokenHash = HashToken(token);
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(lifetime);

        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO ApiTokens (TokenId, UserId, TokenHash, ExpiresAt, CreatedAt)
VALUES ($TokenId, $UserId, $TokenHash, $ExpiresAt, $CreatedAt);
";
        cmd.Parameters.AddWithValue("$TokenId", tokenId.ToString());
        cmd.Parameters.AddWithValue("$UserId", userId.ToString());
        cmd.Parameters.AddWithValue("$TokenHash", tokenHash);
        cmd.Parameters.AddWithValue("$ExpiresAt", expiresAt.ToString("o"));
        cmd.Parameters.AddWithValue("$CreatedAt", now.ToString("o"));
        cmd.ExecuteNonQuery();

        return (token, expiresAt);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var tokenHash = HashToken(token);

        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT COUNT(1)
FROM ApiTokens
WHERE TokenHash = $TokenHash
  AND ExpiresAt > $Now;
";
        cmd.Parameters.AddWithValue("$TokenHash", tokenHash);
        cmd.Parameters.AddWithValue("$Now", DateTime.UtcNow.ToString("o"));

        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Convert.FromBase64String(token));
        return Convert.ToBase64String(hash);
    }
}

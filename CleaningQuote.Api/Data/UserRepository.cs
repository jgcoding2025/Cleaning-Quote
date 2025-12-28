using System.Security.Cryptography;

namespace CleaningQuote.Api.Data;

public sealed class UserRepository
{
    private readonly ApiDb _db;

    public UserRepository(ApiDb db)
    {
        _db = db;
    }

    public bool HasAnyUsers()
    {
        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM ApiUsers;";
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public Guid? ValidateCredentials(string email, string password)
    {
        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT UserId, PasswordHash, PasswordSalt FROM ApiUsers WHERE Email = $Email;";
        cmd.Parameters.AddWithValue("$Email", email.Trim().ToLowerInvariant());

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var userId = Guid.Parse(reader.GetString(0));
        var storedHash = reader.GetString(1);
        var storedSalt = reader.GetString(2);

        var candidate = HashPassword(password, Convert.FromBase64String(storedSalt));
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(storedHash),
            Convert.FromBase64String(candidate))
            ? userId
            : null;
    }

    public Guid CreateUser(string email, string password)
    {
        var userId = Guid.NewGuid();
        var salt = RandomNumberGenerator.GetBytes(32);
        var hashed = HashPassword(password, salt);
        var now = DateTime.UtcNow.ToString("o");

        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO ApiUsers (UserId, Email, PasswordHash, PasswordSalt, CreatedAt)
VALUES ($UserId, $Email, $PasswordHash, $PasswordSalt, $CreatedAt);
";
        cmd.Parameters.AddWithValue("$UserId", userId.ToString());
        cmd.Parameters.AddWithValue("$Email", email.Trim().ToLowerInvariant());
        cmd.Parameters.AddWithValue("$PasswordHash", hashed);
        cmd.Parameters.AddWithValue("$PasswordSalt", Convert.ToBase64String(salt));
        cmd.Parameters.AddWithValue("$CreatedAt", now);
        cmd.ExecuteNonQuery();

        return userId;
    }

    private static string HashPassword(string password, byte[] salt)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(deriveBytes.GetBytes(32));
    }
}

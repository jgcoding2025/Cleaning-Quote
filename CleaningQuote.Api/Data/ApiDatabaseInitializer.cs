namespace CleaningQuote.Api.Data;

public static class ApiDatabaseInitializer
{
    public static void Initialize(ApiDb db)
    {
        using var connection = db.OpenConnection();
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS ApiUsers(
    UserId TEXT PRIMARY KEY,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    PasswordSalt TEXT NOT NULL,
    CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ApiTokens(
    TokenId TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    TokenHash TEXT NOT NULL,
    ExpiresAt TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES ApiUsers(UserId)
);
";
        cmd.ExecuteNonQuery();
    }
}

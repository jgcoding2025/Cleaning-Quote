using Microsoft.Data.Sqlite;

namespace CleaningQuote.Api.Data;

public sealed class ApiDb
{
    private readonly string _connectionString;

    public ApiDb(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection OpenConnection()
    {
        var builder = new SqliteConnectionStringBuilder(_connectionString)
        {
            DefaultTimeout = 5
        };
        var connection = new SqliteConnection(builder.ConnectionString);
        connection.Open();
        return connection;
    }
}

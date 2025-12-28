using Microsoft.Data.Sqlite;

namespace Cleaning_Quote.Data
{
    public static class Db
    {
        // This creates the DB file in your app folder (simple for now)
        public static readonly string ConnectionString = "Data Source=CleaningQuotesTracker.db;Cache=Shared;Mode=ReadWriteCreate";

        public static SqliteConnection OpenConnection()
        {
            var builder = new SqliteConnectionStringBuilder(ConnectionString)
            {
                DefaultTimeout = 5
            };
            var conn = new SqliteConnection(builder.ConnectionString);
            conn.Open();
            return conn;
        }
    }
}

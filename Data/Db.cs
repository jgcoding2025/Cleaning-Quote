using Microsoft.Data.Sqlite;

namespace Cleaning_Quote.Data
{
    public static class Db
    {
        // This creates the DB file in your app folder (simple for now)
        public static readonly string ConnectionString = "Data Source=CleaningQuotesTracker.db;Cache=Shared;Mode=ReadWriteCreate;BusyTimeout=5000";

        public static SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}

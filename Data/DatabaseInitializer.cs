using Microsoft.Data.Sqlite;

namespace Cleaning_Quote.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using var conn = Db.OpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Clients(
    ClientId TEXT PRIMARY KEY,
    DisplayName TEXT NOT NULL,
    AddressLine1 TEXT NOT NULL,
    AddressLine2 TEXT,
    City TEXT NOT NULL,
    State TEXT NOT NULL,
    Zip TEXT NOT NULL,
    Phone TEXT,
    Email TEXT,
    Notes TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Quotes(
    QuoteId TEXT PRIMARY KEY,
    ClientId TEXT NOT NULL,
    QuoteDate TEXT NOT NULL,
    QuoteName TEXT,
    ServiceType TEXT,
    ServiceFrequency TEXT,
    LastProfessionalCleaning TEXT,
    Status TEXT NOT NULL DEFAULT 'Draft',

    LaborRate REAL NOT NULL,
    TaxRate REAL NOT NULL,
    CreditCardFeeRate REAL NOT NULL,
    CreditCard INTEGER NOT NULL,

    PetsCount INTEGER NOT NULL,
    HouseholdSize INTEGER NOT NULL,
    SmokingInside INTEGER NOT NULL,

    TotalLaborHours REAL NOT NULL,
    Subtotal REAL NOT NULL,
    CreditCardFee REAL NOT NULL,
    Tax REAL NOT NULL,
    Total REAL NOT NULL,

    Notes TEXT,

    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,

    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

CREATE TABLE IF NOT EXISTS QuoteRooms(
    QuoteRoomId TEXT PRIMARY KEY,
    QuoteId TEXT NOT NULL,

    RoomType TEXT NOT NULL,
    Size TEXT NOT NULL,
    Complexity INTEGER NOT NULL,

    FullGlassShowersCount INTEGER NOT NULL,
    PebbleStoneFloorsCount INTEGER NOT NULL,
    FridgeCount INTEGER NOT NULL,
    OvenCount INTEGER NOT NULL,

    RoomLaborHours REAL NOT NULL,

    FOREIGN KEY (QuoteId) REFERENCES Quotes(QuoteId)
);

CREATE TABLE IF NOT EXISTS ServiceTypePricing(
    ServiceType TEXT PRIMARY KEY,
    SqFtPerLaborHour REAL NOT NULL,
    SizeSmallSqFt REAL NOT NULL,
    SizeMediumSqFt REAL NOT NULL,
    SizeLargeSqFt REAL NOT NULL,
    Complexity1Multiplier REAL NOT NULL,
    Complexity2Multiplier REAL NOT NULL,
    Complexity3Multiplier REAL NOT NULL,
    FullGlassShowerHoursEach REAL NOT NULL,
    PebbleStoneFloorHoursEach REAL NOT NULL,
    FridgeHoursEach REAL NOT NULL,
    OvenHoursEach REAL NOT NULL,
    UpdatedAt TEXT NOT NULL
);
";
            cmd.ExecuteNonQuery();

            EnsureColumn(conn, "Quotes", "QuoteName", "TEXT");
            EnsureColumn(conn, "Quotes", "ServiceType", "TEXT");
            EnsureColumn(conn, "Quotes", "ServiceFrequency", "TEXT");
            EnsureColumn(conn, "Quotes", "LastProfessionalCleaning", "TEXT");

            EnsureColumn(conn, "ServiceTypePricing", "SqFtPerLaborHour", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeSmallSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeMediumSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeLargeSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity1Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity2Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity3Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "FullGlassShowerHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "PebbleStoneFloorHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "FridgeHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "OvenHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "UpdatedAt", "TEXT");
        }

        private static void EnsureColumn(SqliteConnection conn, string tableName, string columnName, string columnDefinition)
        {
            using var pragma = conn.CreateCommand();
            pragma.CommandText = $"PRAGMA table_info({tableName});";

            using var reader = pragma.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
                    return;
            }

            using var alter = conn.CreateCommand();
            alter.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};";
            alter.ExecuteNonQuery();
        }
    }
}

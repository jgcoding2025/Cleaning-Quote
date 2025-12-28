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
    TotalSqFt REAL NOT NULL DEFAULT 0,
    UseTotalSqFtOverride INTEGER NOT NULL DEFAULT 0,
    EntryInstructions TEXT,
    PaymentMethod TEXT,
    PaymentMethodOther TEXT,
    FeedbackDiscussed INTEGER NOT NULL DEFAULT 0,
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
    ParentRoomId TEXT,

    RoomType TEXT NOT NULL,
    Size TEXT NOT NULL,
    Complexity INTEGER NOT NULL,
    Level TEXT,
    ItemCategory TEXT,
    IsSubItem INTEGER NOT NULL DEFAULT 0,
    IncludedInQuote INTEGER NOT NULL DEFAULT 1,
    WindowInside INTEGER NOT NULL DEFAULT 0,
    WindowOutside INTEGER NOT NULL DEFAULT 0,
    WindowSide TEXT NOT NULL DEFAULT '',
    SortOrder INTEGER NOT NULL DEFAULT 0,

    FullGlassShowersCount INTEGER NOT NULL,
    PebbleStoneFloorsCount INTEGER NOT NULL,
    FridgeCount INTEGER NOT NULL,
    OvenCount INTEGER NOT NULL,

    RoomLaborHours REAL NOT NULL,
    RoomAmount REAL NOT NULL DEFAULT 0,
    RoomNotes TEXT,

    FOREIGN KEY (QuoteId) REFERENCES Quotes(QuoteId)
);

CREATE TABLE IF NOT EXISTS QuotePets(
    QuotePetId TEXT PRIMARY KEY,
    QuoteId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Type TEXT NOT NULL,
    Notes TEXT,
    FOREIGN KEY (QuoteId) REFERENCES Quotes(QuoteId)
);

CREATE TABLE IF NOT EXISTS QuoteOccupants(
    QuoteOccupantId TEXT PRIMARY KEY,
    QuoteId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Relationship TEXT NOT NULL,
    Notes TEXT,
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
    Complexity1Definition TEXT NOT NULL,
    Complexity2Definition TEXT NOT NULL,
    Complexity3Definition TEXT NOT NULL,
    FullGlassShowerHoursEach REAL NOT NULL,
    FullGlassShowerComplexity INTEGER NOT NULL DEFAULT 2,
    PebbleStoneFloorHoursEach REAL NOT NULL,
    PebbleStoneFloorComplexity INTEGER NOT NULL DEFAULT 2,
    FridgeHoursEach REAL NOT NULL,
    FridgeComplexity INTEGER NOT NULL DEFAULT 2,
    OvenHoursEach REAL NOT NULL,
    OvenComplexity INTEGER NOT NULL DEFAULT 2,
    CeilingFanHoursEach REAL NOT NULL DEFAULT 0.15,
    CeilingFanComplexity INTEGER NOT NULL DEFAULT 1,
    WindowSmallHoursEach REAL NOT NULL DEFAULT 0.08,
    WindowMediumHoursEach REAL NOT NULL DEFAULT 0.12,
    WindowLargeHoursEach REAL NOT NULL DEFAULT 0.18,
    WindowComplexity INTEGER NOT NULL DEFAULT 1,
    FirstCleanRate REAL NOT NULL DEFAULT 0.15,
    FirstCleanMinimum REAL NOT NULL DEFAULT 195,
    DeepCleanRate REAL NOT NULL DEFAULT 0.20,
    DeepCleanMinimum REAL NOT NULL DEFAULT 295,
    MaintenanceRate REAL NOT NULL DEFAULT 0.10,
    MaintenanceMinimum REAL NOT NULL DEFAULT 125,
    OneTimeDeepCleanRate REAL NOT NULL DEFAULT 0.30,
    OneTimeDeepCleanMinimum REAL NOT NULL DEFAULT 400,
    WindowInsideRate REAL NOT NULL DEFAULT 4,
    WindowOutsideRate REAL NOT NULL DEFAULT 4,
    DefaultRoomType TEXT NOT NULL DEFAULT 'Bedroom',
    DefaultRoomLevel TEXT NOT NULL DEFAULT 'Main Floor (1)',
    DefaultRoomSize TEXT NOT NULL DEFAULT 'M',
    DefaultRoomComplexity INTEGER NOT NULL DEFAULT 1,
    DefaultSubItemType TEXT NOT NULL DEFAULT 'Ceiling Fan',
    DefaultWindowSize TEXT NOT NULL DEFAULT 'M',
    UpdatedAt TEXT NOT NULL
);
";
            cmd.ExecuteNonQuery();

            EnsureColumn(conn, "Quotes", "QuoteName", "TEXT");
            EnsureColumn(conn, "Quotes", "ServiceType", "TEXT");
            EnsureColumn(conn, "Quotes", "ServiceFrequency", "TEXT");
            EnsureColumn(conn, "Quotes", "LastProfessionalCleaning", "TEXT");
            EnsureColumn(conn, "Quotes", "TotalSqFt", "REAL");
            EnsureColumn(conn, "Quotes", "UseTotalSqFtOverride", "INTEGER");
            EnsureColumn(conn, "Quotes", "EntryInstructions", "TEXT");
            EnsureColumn(conn, "Quotes", "PaymentMethod", "TEXT");
            EnsureColumn(conn, "Quotes", "PaymentMethodOther", "TEXT");
            EnsureColumn(conn, "Quotes", "FeedbackDiscussed", "INTEGER");

            EnsureColumn(conn, "QuoteRooms", "ParentRoomId", "TEXT");
            EnsureColumn(conn, "QuoteRooms", "Level", "TEXT");
            EnsureColumn(conn, "QuoteRooms", "ItemCategory", "TEXT");
            EnsureColumn(conn, "QuoteRooms", "IsSubItem", "INTEGER");
            EnsureColumn(conn, "QuoteRooms", "IncludedInQuote", "INTEGER");
            EnsureColumn(conn, "QuoteRooms", "WindowInside", "INTEGER");
            EnsureColumn(conn, "QuoteRooms", "WindowOutside", "INTEGER");
            EnsureColumn(conn, "QuoteRooms", "WindowSide", "TEXT");
            EnsureColumn(conn, "QuoteRooms", "SortOrder", "INTEGER");
            EnsureColumn(conn, "QuoteRooms", "RoomAmount", "REAL");
            EnsureColumn(conn, "QuoteRooms", "RoomNotes", "TEXT");

            EnsureColumn(conn, "ServiceTypePricing", "SqFtPerLaborHour", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeSmallSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeMediumSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeLargeSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity1Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity2Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity3Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity1Definition", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity2Definition", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity3Definition", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "FullGlassShowerHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "FullGlassShowerComplexity", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "PebbleStoneFloorHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "PebbleStoneFloorComplexity", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "FridgeHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "FridgeComplexity", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "OvenHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "OvenComplexity", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "CeilingFanHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "CeilingFanComplexity", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "WindowSmallHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "WindowMediumHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "WindowLargeHoursEach", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "WindowComplexity", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "FirstCleanRate", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "FirstCleanMinimum", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "DeepCleanRate", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "DeepCleanMinimum", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "MaintenanceRate", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "MaintenanceMinimum", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "OneTimeDeepCleanRate", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "OneTimeDeepCleanMinimum", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "WindowInsideRate", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "WindowOutsideRate", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "DefaultRoomType", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "DefaultRoomLevel", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "DefaultRoomSize", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "DefaultRoomComplexity", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "DefaultSubItemType", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "DefaultWindowSize", "TEXT");
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

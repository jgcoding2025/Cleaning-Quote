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
";
            cmd.ExecuteNonQuery();
        }
    }
}

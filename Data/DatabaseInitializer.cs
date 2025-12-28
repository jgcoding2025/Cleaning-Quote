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
    SortOrder INTEGER NOT NULL DEFAULT 0,

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
    SizeSmallMultiplier REAL NOT NULL DEFAULT 0.9,
    SizeMediumMultiplier REAL NOT NULL DEFAULT 1.0,
    SizeLargeMultiplier REAL NOT NULL DEFAULT 1.1,
    SizeSmallDefinition TEXT NOT NULL DEFAULT 'Small',
    SizeMediumDefinition TEXT NOT NULL DEFAULT 'Medium (average)',
    SizeLargeDefinition TEXT NOT NULL DEFAULT 'Large',
    Complexity1Multiplier REAL NOT NULL,
    Complexity2Multiplier REAL NOT NULL,
    Complexity3Multiplier REAL NOT NULL,
    Complexity1Definition TEXT NOT NULL,
    Complexity2Definition TEXT NOT NULL,
    Complexity3Definition TEXT NOT NULL,
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
    RoomBathroomFullMinutes INTEGER NOT NULL DEFAULT 60,
    RoomBathroomHalfMinutes INTEGER NOT NULL DEFAULT 20,
    RoomBathroomMasterMinutes INTEGER NOT NULL DEFAULT 120,
    RoomBedroomMinutes INTEGER NOT NULL DEFAULT 15,
    RoomBedroomMasterMinutes INTEGER NOT NULL DEFAULT 20,
    RoomDiningRoomMinutes INTEGER NOT NULL DEFAULT 20,
    RoomEntryMinutes INTEGER NOT NULL DEFAULT 8,
    RoomFamilyRoomMinutes INTEGER NOT NULL DEFAULT 20,
    RoomHallwayMinutes INTEGER NOT NULL DEFAULT 6,
    RoomKitchenMinutes INTEGER NOT NULL DEFAULT 60,
    RoomLaundryMinutes INTEGER NOT NULL DEFAULT 10,
    RoomLivingRoomMinutes INTEGER NOT NULL DEFAULT 20,
    RoomOfficeMinutes INTEGER NOT NULL DEFAULT 15,
    RoomBathroomFullSqFt INTEGER NOT NULL DEFAULT 50,
    RoomBathroomHalfSqFt INTEGER NOT NULL DEFAULT 25,
    RoomBathroomMasterSqFt INTEGER NOT NULL DEFAULT 100,
    RoomBedroomSqFt INTEGER NOT NULL DEFAULT 110,
    RoomBedroomMasterSqFt INTEGER NOT NULL DEFAULT 225,
    RoomDiningRoomSqFt INTEGER NOT NULL DEFAULT 140,
    RoomEntrySqFt INTEGER NOT NULL DEFAULT 45,
    RoomFamilyRoomSqFt INTEGER NOT NULL DEFAULT 300,
    RoomHallwaySqFt INTEGER NOT NULL DEFAULT 40,
    RoomKitchenSqFt INTEGER NOT NULL DEFAULT 175,
    RoomLaundrySqFt INTEGER NOT NULL DEFAULT 55,
    RoomLivingRoomSqFt INTEGER NOT NULL DEFAULT 300,
    RoomOfficeSqFt INTEGER NOT NULL DEFAULT 120,
    SubItemCeilingFanMinutes INTEGER NOT NULL DEFAULT 10,
    SubItemFridgeMinutes INTEGER NOT NULL DEFAULT 60,
    SubItemMirrorMinutes INTEGER NOT NULL DEFAULT 5,
    SubItemOvenMinutes INTEGER NOT NULL DEFAULT 60,
    SubItemShowerNoGlassMinutes INTEGER NOT NULL DEFAULT -20,
    SubItemShowerNoStoneMinutes INTEGER NOT NULL DEFAULT -20,
    SubItemSinkDiscountMinutes INTEGER NOT NULL DEFAULT -10,
    SubItemStoveTopGasMinutes INTEGER NOT NULL DEFAULT 30,
    SubItemTubMinutes INTEGER NOT NULL DEFAULT 25,
    SubItemWindowInsideFirstMinutes INTEGER NOT NULL DEFAULT 5,
    SubItemWindowOutsideFirstMinutes INTEGER NOT NULL DEFAULT 5,
    SubItemWindowInsideSecondMinutes INTEGER NOT NULL DEFAULT 10,
    SubItemWindowOutsideSecondMinutes INTEGER NOT NULL DEFAULT 10,
    SubItemWindowTrackMinutes INTEGER NOT NULL DEFAULT 5,
    SubItemWindowStandardMinutes INTEGER NOT NULL DEFAULT 10,
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
            EnsureColumn(conn, "QuoteRooms", "SortOrder", "INTEGER");
            EnsureColumn(conn, "QuoteRooms", "RoomAmount", "REAL");
            EnsureColumn(conn, "QuoteRooms", "RoomNotes", "TEXT");

            EnsureColumn(conn, "ServiceTypePricing", "SqFtPerLaborHour", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeSmallSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeMediumSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeLargeSqFt", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeSmallMultiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeMediumMultiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeLargeMultiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "SizeSmallDefinition", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "SizeMediumDefinition", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "SizeLargeDefinition", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity1Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity2Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity3Multiplier", "REAL");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity1Definition", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity2Definition", "TEXT");
            EnsureColumn(conn, "ServiceTypePricing", "Complexity3Definition", "TEXT");
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
            EnsureColumn(conn, "ServiceTypePricing", "RoomBathroomFullMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBathroomHalfMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBathroomMasterMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBedroomMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBedroomMasterMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomDiningRoomMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomEntryMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomFamilyRoomMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomHallwayMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomKitchenMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomLaundryMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomLivingRoomMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomOfficeMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBathroomFullSqFt", "INTEGER NOT NULL DEFAULT 50");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBathroomHalfSqFt", "INTEGER NOT NULL DEFAULT 25");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBathroomMasterSqFt", "INTEGER NOT NULL DEFAULT 100");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBedroomSqFt", "INTEGER NOT NULL DEFAULT 110");
            EnsureColumn(conn, "ServiceTypePricing", "RoomBedroomMasterSqFt", "INTEGER NOT NULL DEFAULT 225");
            EnsureColumn(conn, "ServiceTypePricing", "RoomDiningRoomSqFt", "INTEGER NOT NULL DEFAULT 140");
            EnsureColumn(conn, "ServiceTypePricing", "RoomEntrySqFt", "INTEGER NOT NULL DEFAULT 45");
            EnsureColumn(conn, "ServiceTypePricing", "RoomFamilyRoomSqFt", "INTEGER NOT NULL DEFAULT 300");
            EnsureColumn(conn, "ServiceTypePricing", "RoomHallwaySqFt", "INTEGER NOT NULL DEFAULT 40");
            EnsureColumn(conn, "ServiceTypePricing", "RoomKitchenSqFt", "INTEGER NOT NULL DEFAULT 175");
            EnsureColumn(conn, "ServiceTypePricing", "RoomLaundrySqFt", "INTEGER NOT NULL DEFAULT 55");
            EnsureColumn(conn, "ServiceTypePricing", "RoomLivingRoomSqFt", "INTEGER NOT NULL DEFAULT 300");
            EnsureColumn(conn, "ServiceTypePricing", "RoomOfficeSqFt", "INTEGER NOT NULL DEFAULT 120");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemCeilingFanMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemFridgeMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemMirrorMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemOvenMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemShowerNoGlassMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemShowerNoStoneMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemSinkDiscountMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemStoveTopGasMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemTubMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemWindowInsideFirstMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemWindowOutsideFirstMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemWindowInsideSecondMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemWindowOutsideSecondMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemWindowTrackMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "SubItemWindowStandardMinutes", "INTEGER");
            EnsureColumn(conn, "ServiceTypePricing", "UpdatedAt", "TEXT");

            RebuildQuoteRoomsIfNeeded(conn);
            RebuildServiceTypePricingIfNeeded(conn);
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

        private static bool ColumnExists(SqliteConnection conn, string tableName, string columnName)
        {
            using var pragma = conn.CreateCommand();
            pragma.CommandText = $"PRAGMA table_info({tableName});";

            using var reader = pragma.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void RebuildQuoteRoomsIfNeeded(SqliteConnection conn)
        {
            if (!ColumnExists(conn, "QuoteRooms", "WindowSide") &&
                !ColumnExists(conn, "QuoteRooms", "WindowInside") &&
                !ColumnExists(conn, "QuoteRooms", "WindowOutside") &&
                !ColumnExists(conn, "QuoteRooms", "FullGlassShowersCount") &&
                !ColumnExists(conn, "QuoteRooms", "PebbleStoneFloorsCount") &&
                !ColumnExists(conn, "QuoteRooms", "FridgeCount") &&
                !ColumnExists(conn, "QuoteRooms", "OvenCount"))
            {
                return;
            }

            using var tx = conn.BeginTransaction();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS QuoteRooms_New(
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
    SortOrder INTEGER NOT NULL DEFAULT 0,

    RoomLaborHours REAL NOT NULL,
    RoomAmount REAL NOT NULL DEFAULT 0,
    RoomNotes TEXT,

    FOREIGN KEY (QuoteId) REFERENCES Quotes(QuoteId)
);
";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO QuoteRooms_New
(QuoteRoomId, QuoteId, ParentRoomId,
 RoomType, Size, Complexity, Level, ItemCategory, IsSubItem, IncludedInQuote, SortOrder,
 RoomLaborHours, RoomAmount, RoomNotes)
SELECT QuoteRoomId, QuoteId, ParentRoomId,
       RoomType, Size, Complexity, Level, ItemCategory, IsSubItem, IncludedInQuote, COALESCE(SortOrder, 0),
       RoomLaborHours, RoomAmount, RoomNotes
FROM QuoteRooms;
";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DROP TABLE QuoteRooms;";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "ALTER TABLE QuoteRooms_New RENAME TO QuoteRooms;";
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        private static void RebuildServiceTypePricingIfNeeded(SqliteConnection conn)
        {
            if (!ColumnExists(conn, "ServiceTypePricing", "FullGlassShowerHoursEach") &&
                !ColumnExists(conn, "ServiceTypePricing", "PebbleStoneFloorHoursEach") &&
                !ColumnExists(conn, "ServiceTypePricing", "FridgeHoursEach") &&
                !ColumnExists(conn, "ServiceTypePricing", "OvenHoursEach") &&
                !ColumnExists(conn, "ServiceTypePricing", "CeilingFanHoursEach") &&
                !ColumnExists(conn, "ServiceTypePricing", "WindowSmallHoursEach") &&
                !ColumnExists(conn, "ServiceTypePricing", "WindowMediumHoursEach") &&
                !ColumnExists(conn, "ServiceTypePricing", "WindowLargeHoursEach"))
            {
                return;
            }

            using var tx = conn.BeginTransaction();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS ServiceTypePricing_New(
    ServiceType TEXT PRIMARY KEY,
    SqFtPerLaborHour REAL NOT NULL,
    SizeSmallSqFt REAL NOT NULL,
    SizeMediumSqFt REAL NOT NULL,
    SizeLargeSqFt REAL NOT NULL,
    SizeSmallMultiplier REAL NOT NULL DEFAULT 0.9,
    SizeMediumMultiplier REAL NOT NULL DEFAULT 1.0,
    SizeLargeMultiplier REAL NOT NULL DEFAULT 1.1,
    SizeSmallDefinition TEXT NOT NULL DEFAULT 'Small',
    SizeMediumDefinition TEXT NOT NULL DEFAULT 'Medium (average)',
    SizeLargeDefinition TEXT NOT NULL DEFAULT 'Large',
    Complexity1Multiplier REAL NOT NULL,
    Complexity2Multiplier REAL NOT NULL,
    Complexity3Multiplier REAL NOT NULL,
    Complexity1Definition TEXT NOT NULL,
    Complexity2Definition TEXT NOT NULL,
    Complexity3Definition TEXT NOT NULL,
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
    RoomBathroomFullMinutes INTEGER NOT NULL DEFAULT 60,
    RoomBathroomHalfMinutes INTEGER NOT NULL DEFAULT 20,
    RoomBathroomMasterMinutes INTEGER NOT NULL DEFAULT 120,
    RoomBedroomMinutes INTEGER NOT NULL DEFAULT 15,
    RoomBedroomMasterMinutes INTEGER NOT NULL DEFAULT 20,
    RoomDiningRoomMinutes INTEGER NOT NULL DEFAULT 20,
    RoomEntryMinutes INTEGER NOT NULL DEFAULT 8,
    RoomFamilyRoomMinutes INTEGER NOT NULL DEFAULT 20,
    RoomHallwayMinutes INTEGER NOT NULL DEFAULT 6,
    RoomKitchenMinutes INTEGER NOT NULL DEFAULT 60,
    RoomLaundryMinutes INTEGER NOT NULL DEFAULT 10,
    RoomLivingRoomMinutes INTEGER NOT NULL DEFAULT 20,
    RoomOfficeMinutes INTEGER NOT NULL DEFAULT 15,
    RoomBathroomFullSqFt INTEGER NOT NULL DEFAULT 50,
    RoomBathroomHalfSqFt INTEGER NOT NULL DEFAULT 25,
    RoomBathroomMasterSqFt INTEGER NOT NULL DEFAULT 100,
    RoomBedroomSqFt INTEGER NOT NULL DEFAULT 110,
    RoomBedroomMasterSqFt INTEGER NOT NULL DEFAULT 225,
    RoomDiningRoomSqFt INTEGER NOT NULL DEFAULT 140,
    RoomEntrySqFt INTEGER NOT NULL DEFAULT 45,
    RoomFamilyRoomSqFt INTEGER NOT NULL DEFAULT 300,
    RoomHallwaySqFt INTEGER NOT NULL DEFAULT 40,
    RoomKitchenSqFt INTEGER NOT NULL DEFAULT 175,
    RoomLaundrySqFt INTEGER NOT NULL DEFAULT 55,
    RoomLivingRoomSqFt INTEGER NOT NULL DEFAULT 300,
    RoomOfficeSqFt INTEGER NOT NULL DEFAULT 120,
    SubItemCeilingFanMinutes INTEGER NOT NULL DEFAULT 10,
    SubItemFridgeMinutes INTEGER NOT NULL DEFAULT 60,
    SubItemMirrorMinutes INTEGER NOT NULL DEFAULT 5,
    SubItemOvenMinutes INTEGER NOT NULL DEFAULT 60,
    SubItemShowerNoGlassMinutes INTEGER NOT NULL DEFAULT -20,
    SubItemShowerNoStoneMinutes INTEGER NOT NULL DEFAULT -20,
    SubItemSinkDiscountMinutes INTEGER NOT NULL DEFAULT -10,
    SubItemStoveTopGasMinutes INTEGER NOT NULL DEFAULT 30,
    SubItemTubMinutes INTEGER NOT NULL DEFAULT 25,
    SubItemWindowInsideFirstMinutes INTEGER NOT NULL DEFAULT 5,
    SubItemWindowOutsideFirstMinutes INTEGER NOT NULL DEFAULT 5,
    SubItemWindowInsideSecondMinutes INTEGER NOT NULL DEFAULT 10,
    SubItemWindowOutsideSecondMinutes INTEGER NOT NULL DEFAULT 10,
    SubItemWindowTrackMinutes INTEGER NOT NULL DEFAULT 5,
    SubItemWindowStandardMinutes INTEGER NOT NULL DEFAULT 10,
    UpdatedAt TEXT NOT NULL
);
";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO ServiceTypePricing_New
(ServiceType, SqFtPerLaborHour, SizeSmallSqFt, SizeMediumSqFt, SizeLargeSqFt,
 SizeSmallMultiplier, SizeMediumMultiplier, SizeLargeMultiplier,
 SizeSmallDefinition, SizeMediumDefinition, SizeLargeDefinition,
 Complexity1Multiplier, Complexity2Multiplier, Complexity3Multiplier,
 Complexity1Definition, Complexity2Definition, Complexity3Definition,
 FirstCleanRate, FirstCleanMinimum,
 DeepCleanRate, DeepCleanMinimum,
 MaintenanceRate, MaintenanceMinimum,
 OneTimeDeepCleanRate, OneTimeDeepCleanMinimum,
 WindowInsideRate, WindowOutsideRate,
 DefaultRoomType, DefaultRoomLevel, DefaultRoomSize, DefaultRoomComplexity,
 DefaultSubItemType, DefaultWindowSize,
 RoomBathroomFullMinutes, RoomBathroomHalfMinutes, RoomBathroomMasterMinutes,
 RoomBedroomMinutes, RoomBedroomMasterMinutes, RoomDiningRoomMinutes,
 RoomEntryMinutes, RoomFamilyRoomMinutes, RoomHallwayMinutes,
 RoomKitchenMinutes, RoomLaundryMinutes, RoomLivingRoomMinutes, RoomOfficeMinutes,
 RoomBathroomFullSqFt, RoomBathroomHalfSqFt, RoomBathroomMasterSqFt,
 RoomBedroomSqFt, RoomBedroomMasterSqFt, RoomDiningRoomSqFt,
 RoomEntrySqFt, RoomFamilyRoomSqFt, RoomHallwaySqFt,
 RoomKitchenSqFt, RoomLaundrySqFt, RoomLivingRoomSqFt, RoomOfficeSqFt,
 SubItemCeilingFanMinutes, SubItemFridgeMinutes, SubItemMirrorMinutes, SubItemOvenMinutes,
 SubItemShowerNoGlassMinutes, SubItemShowerNoStoneMinutes, SubItemSinkDiscountMinutes,
 SubItemStoveTopGasMinutes, SubItemTubMinutes,
 SubItemWindowInsideFirstMinutes, SubItemWindowOutsideFirstMinutes,
 SubItemWindowInsideSecondMinutes, SubItemWindowOutsideSecondMinutes,
 SubItemWindowTrackMinutes, SubItemWindowStandardMinutes,
 UpdatedAt)
SELECT ServiceType,
       COALESCE(SqFtPerLaborHour, 500), COALESCE(SizeSmallSqFt, 250), COALESCE(SizeMediumSqFt, 375), COALESCE(SizeLargeSqFt, 500),
       COALESCE(SizeSmallMultiplier, 0.9), COALESCE(SizeMediumMultiplier, 1.0), COALESCE(SizeLargeMultiplier, 1.1),
       COALESCE(SizeSmallDefinition, 'Small'), COALESCE(SizeMediumDefinition, 'Medium (average)'), COALESCE(SizeLargeDefinition, 'Large'),
       COALESCE(Complexity1Multiplier, 0.75), COALESCE(Complexity2Multiplier, 1.0), COALESCE(Complexity3Multiplier, 1.25),
       COALESCE(Complexity1Definition, 'Light use with minimal buildup.'),
       COALESCE(Complexity2Definition, 'Moderate use with visible buildup (Average).'),
       COALESCE(Complexity3Definition, 'Heavy use with significant buildup.'),
       COALESCE(FirstCleanRate, 0.15), COALESCE(FirstCleanMinimum, 195),
       COALESCE(DeepCleanRate, 0.20), COALESCE(DeepCleanMinimum, 295),
       COALESCE(MaintenanceRate, 0.10), COALESCE(MaintenanceMinimum, 125),
       COALESCE(OneTimeDeepCleanRate, 0.30), COALESCE(OneTimeDeepCleanMinimum, 400),
       COALESCE(WindowInsideRate, 4), COALESCE(WindowOutsideRate, 4),
       COALESCE(DefaultRoomType, 'Bedroom'), COALESCE(DefaultRoomLevel, 'Main Floor (1)'), COALESCE(DefaultRoomSize, 'M'), COALESCE(DefaultRoomComplexity, 2),
       COALESCE(DefaultSubItemType, 'Ceiling Fan'), COALESCE(DefaultWindowSize, 'M'),
       COALESCE(RoomBathroomFullMinutes, 60), COALESCE(RoomBathroomHalfMinutes, 20), COALESCE(RoomBathroomMasterMinutes, 120),
       COALESCE(RoomBedroomMinutes, 15), COALESCE(RoomBedroomMasterMinutes, 20), COALESCE(RoomDiningRoomMinutes, 20),
       COALESCE(RoomEntryMinutes, 8), COALESCE(RoomFamilyRoomMinutes, 20), COALESCE(RoomHallwayMinutes, 6),
       COALESCE(RoomKitchenMinutes, 60), COALESCE(RoomLaundryMinutes, 10), COALESCE(RoomLivingRoomMinutes, 20), COALESCE(RoomOfficeMinutes, 15),
       COALESCE(RoomBathroomFullSqFt, 50), COALESCE(RoomBathroomHalfSqFt, 25), COALESCE(RoomBathroomMasterSqFt, 100),
       COALESCE(RoomBedroomSqFt, 110), COALESCE(RoomBedroomMasterSqFt, 225), COALESCE(RoomDiningRoomSqFt, 140),
       COALESCE(RoomEntrySqFt, 45), COALESCE(RoomFamilyRoomSqFt, 300), COALESCE(RoomHallwaySqFt, 40),
       COALESCE(RoomKitchenSqFt, 175), COALESCE(RoomLaundrySqFt, 55), COALESCE(RoomLivingRoomSqFt, 300), COALESCE(RoomOfficeSqFt, 120),
       COALESCE(SubItemCeilingFanMinutes, 10), COALESCE(SubItemFridgeMinutes, 60), COALESCE(SubItemMirrorMinutes, 5), COALESCE(SubItemOvenMinutes, 60),
       COALESCE(SubItemShowerNoGlassMinutes, -20), COALESCE(SubItemShowerNoStoneMinutes, -20), COALESCE(SubItemSinkDiscountMinutes, -10),
       COALESCE(SubItemStoveTopGasMinutes, 30), COALESCE(SubItemTubMinutes, 25),
       COALESCE(SubItemWindowInsideFirstMinutes, 5), COALESCE(SubItemWindowOutsideFirstMinutes, 5),
       COALESCE(SubItemWindowInsideSecondMinutes, 10), COALESCE(SubItemWindowOutsideSecondMinutes, 10),
       COALESCE(SubItemWindowTrackMinutes, 5), COALESCE(SubItemWindowStandardMinutes, 10),
       COALESCE(UpdatedAt, CURRENT_TIMESTAMP)
FROM ServiceTypePricing;
";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DROP TABLE ServiceTypePricing;";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "ALTER TABLE ServiceTypePricing_New RENAME TO ServiceTypePricing;";
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
    }
}

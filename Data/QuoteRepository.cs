using Cleaning_Quote.Models;
using System;
using System.Collections.Generic;

namespace Cleaning_Quote.Data
{
    public class QuoteRepository
    {
        public void Insert(Quote quote)
        {
            using var conn = Db.OpenConnection();
            using var tx = conn.BeginTransaction();

            var now = DateTime.UtcNow.ToString("o"); // ISO-8601 (UTC)

            // 1) Quotes row
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;

                cmd.CommandText = @"
INSERT INTO Quotes
(QuoteId, ClientId, QuoteDate, QuoteName, ServiceType, ServiceFrequency, LastProfessionalCleaning,
 TotalSqFt, UseTotalSqFtOverride, EntryInstructions, PaymentMethod, PaymentMethodOther, FeedbackDiscussed,
 Status, LaborRate, TaxRate, CreditCardFeeRate, CreditCard,
 PetsCount, HouseholdSize, SmokingInside, TotalLaborHours, Subtotal, CreditCardFee, Tax, Total,
 Notes, CreatedAt, UpdatedAt)
VALUES
($QuoteId, $ClientId, $QuoteDate, $QuoteName, $ServiceType, $ServiceFrequency, $LastProfessionalCleaning,
 $TotalSqFt, $UseTotalSqFtOverride, $EntryInstructions, $PaymentMethod, $PaymentMethodOther, $FeedbackDiscussed,
 $Status, $LaborRate, $TaxRate, $CreditCardFeeRate, $CreditCard,
 $PetsCount, $HouseholdSize, $SmokingInside, $TotalLaborHours, $Subtotal, $CreditCardFee, $Tax, $Total,
 $Notes, $CreatedAt, $UpdatedAt);
";

                cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                cmd.Parameters.AddWithValue("$ClientId", quote.ClientId.ToString());
                cmd.Parameters.AddWithValue("$QuoteDate", quote.QuoteDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("$QuoteName", quote.QuoteName ?? "");
                cmd.Parameters.AddWithValue("$ServiceType", quote.ServiceType ?? "");
                cmd.Parameters.AddWithValue("$ServiceFrequency", quote.ServiceFrequency ?? "");
                cmd.Parameters.AddWithValue("$LastProfessionalCleaning", quote.LastProfessionalCleaning ?? "");
                cmd.Parameters.AddWithValue("$TotalSqFt", (double)quote.TotalSqFt);
                cmd.Parameters.AddWithValue("$UseTotalSqFtOverride", quote.UseTotalSqFtOverride ? 1 : 0);
                cmd.Parameters.AddWithValue("$EntryInstructions", quote.EntryInstructions ?? "");
                cmd.Parameters.AddWithValue("$PaymentMethod", quote.PaymentMethod ?? "");
                cmd.Parameters.AddWithValue("$PaymentMethodOther", quote.PaymentMethodOther ?? "");
                cmd.Parameters.AddWithValue("$FeedbackDiscussed", quote.FeedbackDiscussed ? 1 : 0);
                cmd.Parameters.AddWithValue("$Status", string.IsNullOrWhiteSpace(quote.Status) ? "Draft" : quote.Status);

                cmd.Parameters.AddWithValue("$LaborRate", (double)quote.LaborRate);
                cmd.Parameters.AddWithValue("$TaxRate", (double)quote.TaxRate);
                cmd.Parameters.AddWithValue("$CreditCardFeeRate", (double)quote.CreditCardFeeRate);
                cmd.Parameters.AddWithValue("$CreditCard", quote.CreditCard ? 1 : 0);

                cmd.Parameters.AddWithValue("$PetsCount", quote.PetsCount);
                cmd.Parameters.AddWithValue("$HouseholdSize", quote.HouseholdSize);
                cmd.Parameters.AddWithValue("$SmokingInside", quote.SmokingInside ? 1 : 0);

                cmd.Parameters.AddWithValue("$TotalLaborHours", (double)quote.TotalLaborHours);
                cmd.Parameters.AddWithValue("$Subtotal", (double)quote.Subtotal);
                cmd.Parameters.AddWithValue("$CreditCardFee", (double)quote.CreditCardFee);
                cmd.Parameters.AddWithValue("$Tax", (double)quote.Tax);
                cmd.Parameters.AddWithValue("$Total", (double)quote.Total);

                cmd.Parameters.AddWithValue("$Notes", quote.Notes ?? "");
                cmd.Parameters.AddWithValue("$CreatedAt", now);
                cmd.Parameters.AddWithValue("$UpdatedAt", now);

                cmd.ExecuteNonQuery();
            } // ✅ closes header insert cmd

            // 2) QuoteRooms rows
            if (quote.Rooms != null)
            {
                foreach (var room in quote.Rooms)
                {
                    using var roomCmd = conn.CreateCommand();
                    roomCmd.Transaction = tx;

                    roomCmd.CommandText = @"
INSERT INTO QuoteRooms
(
    QuoteRoomId, QuoteId, ParentRoomId,
    RoomType, Size, Complexity,
    Level, ItemCategory, IsSubItem, IncludedInQuote, WindowInside, WindowOutside,
    FullGlassShowersCount, PebbleStoneFloorsCount, FridgeCount, OvenCount,
    RoomLaborHours, RoomAmount, RoomNotes
)
VALUES
(
    $QuoteRoomId, $QuoteId, $ParentRoomId,
    $RoomType, $Size, $Complexity,
    $Level, $ItemCategory, $IsSubItem, $IncludedInQuote, $WindowInside, $WindowOutside,
    $Glass, $Pebble, $Fridge, $Oven,
    $RoomHours, $RoomAmount, $RoomNotes
);
";
                    roomCmd.Parameters.AddWithValue("$QuoteRoomId", room.QuoteRoomId.ToString());
                    roomCmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                    roomCmd.Parameters.AddWithValue("$ParentRoomId", (object?)room.ParentRoomId?.ToString() ?? DBNull.Value);
                    roomCmd.Parameters.AddWithValue("$RoomType", room.RoomType ?? "");
                    roomCmd.Parameters.AddWithValue("$Size", room.Size ?? "M");
                    roomCmd.Parameters.AddWithValue("$Complexity", room.Complexity);
                    roomCmd.Parameters.AddWithValue("$Level", room.Level ?? "");
                    roomCmd.Parameters.AddWithValue("$ItemCategory", room.ItemCategory ?? "");
                    roomCmd.Parameters.AddWithValue("$IsSubItem", room.IsSubItem ? 1 : 0);
                    roomCmd.Parameters.AddWithValue("$IncludedInQuote", room.IncludedInQuote ? 1 : 0);
                    roomCmd.Parameters.AddWithValue("$WindowInside", room.WindowInside ? 1 : 0);
                    roomCmd.Parameters.AddWithValue("$WindowOutside", room.WindowOutside ? 1 : 0);
                    roomCmd.Parameters.AddWithValue("$Glass", room.FullGlassShowersCount);
                    roomCmd.Parameters.AddWithValue("$Pebble", room.PebbleStoneFloorsCount);
                    roomCmd.Parameters.AddWithValue("$Fridge", room.FridgeCount);
                    roomCmd.Parameters.AddWithValue("$Oven", room.OvenCount);
                    roomCmd.Parameters.AddWithValue("$RoomHours", (double)room.RoomLaborHours);
                    roomCmd.Parameters.AddWithValue("$RoomAmount", (double)room.RoomAmount);
                    roomCmd.Parameters.AddWithValue("$RoomNotes", room.RoomNotes ?? "");

                    roomCmd.ExecuteNonQuery();
                }
            }

            InsertQuotePets(conn, tx, quote);
            InsertQuoteOccupants(conn, tx, quote);

            tx.Commit();
        }

        public List<QuoteListItem> GetForClient(Guid clientId)
        {
            var list = new List<QuoteListItem>();

            using var conn = Db.OpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
SELECT QuoteId, QuoteDate, Total, TotalLaborHours, QuoteName
FROM Quotes
WHERE ClientId = $ClientId
ORDER BY QuoteDate DESC, CreatedAt DESC;
";
            cmd.Parameters.AddWithValue("$ClientId", clientId.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new QuoteListItem
                {
                    QuoteId = Guid.Parse(reader.GetString(0)),
                    QuoteDate = DateTime.Parse(reader.GetString(1)),
                    Total = Convert.ToDecimal(reader.GetDouble(2)),
                    Hours = Convert.ToDecimal(reader.GetDouble(3)),
                    QuoteName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                });
            }

            return list;
        }

        public Quote GetById(Guid quoteId)
        {
            using var conn = Db.OpenConnection();

            Quote q;

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                SELECT QuoteId, ClientId, QuoteDate, QuoteName, ServiceType, ServiceFrequency, LastProfessionalCleaning,
                       TotalSqFt, UseTotalSqFtOverride, EntryInstructions, PaymentMethod, PaymentMethodOther, FeedbackDiscussed,
                       LaborRate, TaxRate, CreditCardFeeRate, CreditCard,
                       PetsCount, HouseholdSize, SmokingInside,
                       TotalLaborHours, Subtotal, CreditCardFee, Tax, Total,
                       Notes
                FROM Quotes
                WHERE QuoteId = $QuoteId;
                ";
                cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());

                using var r = cmd.ExecuteReader();
                if (!r.Read()) return null;

                q = new Quote
                {
                    QuoteId = Guid.Parse(r.GetString(0)),
                    ClientId = Guid.Parse(r.GetString(1)),
                    QuoteDate = DateTime.Parse(r.GetString(2)),
                    QuoteName = r.IsDBNull(3) ? "" : r.GetString(3),
                    ServiceType = r.IsDBNull(4) ? "" : r.GetString(4),
                    ServiceFrequency = r.IsDBNull(5) ? "" : r.GetString(5),
                    LastProfessionalCleaning = r.IsDBNull(6) ? "" : r.GetString(6),
                    TotalSqFt = r.IsDBNull(7) ? 0m : Convert.ToDecimal(r.GetDouble(7)),
                    UseTotalSqFtOverride = !r.IsDBNull(8) && r.GetInt32(8) == 1,
                    EntryInstructions = r.IsDBNull(9) ? "" : r.GetString(9),
                    PaymentMethod = r.IsDBNull(10) ? "" : r.GetString(10),
                    PaymentMethodOther = r.IsDBNull(11) ? "" : r.GetString(11),
                    FeedbackDiscussed = !r.IsDBNull(12) && r.GetInt32(12) == 1,

                    LaborRate = Convert.ToDecimal(r.GetDouble(13)),
                    TaxRate = Convert.ToDecimal(r.GetDouble(14)),
                    CreditCardFeeRate = Convert.ToDecimal(r.GetDouble(15)),
                    CreditCard = r.GetInt32(16) == 1,

                    PetsCount = r.IsDBNull(17) ? 0 : r.GetInt32(17),
                    HouseholdSize = r.IsDBNull(18) ? 2 : r.GetInt32(18),
                    SmokingInside = !r.IsDBNull(19) && r.GetInt32(19) == 1,

                    TotalLaborHours = Convert.ToDecimal(r.GetDouble(20)),
                    Subtotal = Convert.ToDecimal(r.GetDouble(21)),
                    CreditCardFee = Convert.ToDecimal(r.GetDouble(22)),
                    Tax = Convert.ToDecimal(r.GetDouble(23)),
                    Total = Convert.ToDecimal(r.GetDouble(24)),

                    Notes = r.IsDBNull(25) ? "" : r.GetString(25),
                };
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT QuoteRoomId, RoomType, Size, Complexity,
       Level, ItemCategory, IsSubItem, ParentRoomId, IncludedInQuote, WindowInside, WindowOutside,
       FullGlassShowersCount, PebbleStoneFloorsCount, FridgeCount, OvenCount,
       RoomLaborHours, RoomAmount, RoomNotes
FROM QuoteRooms
WHERE QuoteId = $QuoteId;
";
                cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    q.Rooms.Add(new QuoteRoom
                    {
                        QuoteRoomId = Guid.Parse(r.GetString(0)),
                        QuoteId = quoteId,
                        RoomType = r.GetString(1),
                        Size = r.GetString(2),
                        Complexity = r.GetInt32(3),
                        Level = r.IsDBNull(4) ? "" : r.GetString(4),
                        ItemCategory = r.IsDBNull(5) ? "" : r.GetString(5),
                        IsSubItem = !r.IsDBNull(6) && r.GetInt32(6) == 1,
                        ParentRoomId = r.IsDBNull(7) ? (Guid?)null : Guid.Parse(r.GetString(7)),
                        IncludedInQuote = r.IsDBNull(8) || r.GetInt32(8) == 1,
                        WindowInside = !r.IsDBNull(9) && r.GetInt32(9) == 1,
                        WindowOutside = !r.IsDBNull(10) && r.GetInt32(10) == 1,
                        FullGlassShowersCount = r.IsDBNull(11) ? 0 : r.GetInt32(11),
                        PebbleStoneFloorsCount = r.IsDBNull(12) ? 0 : r.GetInt32(12),
                        FridgeCount = r.IsDBNull(13) ? 0 : r.GetInt32(13),
                        OvenCount = r.IsDBNull(14) ? 0 : r.GetInt32(14),
                        RoomLaborHours = r.IsDBNull(15) ? 0m : Convert.ToDecimal(r.GetDouble(15)),
                        RoomAmount = r.IsDBNull(16) ? 0m : Convert.ToDecimal(r.GetDouble(16)),
                        RoomNotes = r.IsDBNull(17) ? "" : r.GetString(17),
                    });
                }
            }

            LoadQuotePets(conn, q);
            LoadQuoteOccupants(conn, q);

            return q;
        }

        public void Update(Quote quote)
        {
            using var conn = Db.OpenConnection();
            using var tx = conn.BeginTransaction();

            var now = DateTime.UtcNow.ToString("o");

            // 1) Update Quotes header
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;

                cmd.CommandText = @"
UPDATE Quotes
SET
    ClientId = $ClientId,
    QuoteDate = $QuoteDate,
    QuoteName = $QuoteName,
    ServiceType = $ServiceType,
    ServiceFrequency = $ServiceFrequency,
    LastProfessionalCleaning = $LastProfessionalCleaning,
    TotalSqFt = $TotalSqFt,
    UseTotalSqFtOverride = $UseTotalSqFtOverride,
    EntryInstructions = $EntryInstructions,
    PaymentMethod = $PaymentMethod,
    PaymentMethodOther = $PaymentMethodOther,
    FeedbackDiscussed = $FeedbackDiscussed,
    Status = $Status,

    LaborRate = $LaborRate,
    TaxRate = $TaxRate,
    CreditCardFeeRate = $CreditCardFeeRate,
    CreditCard = $CreditCard,

    PetsCount = $PetsCount,
    HouseholdSize = $HouseholdSize,
    SmokingInside = $SmokingInside,

    TotalLaborHours = $TotalLaborHours,
    Subtotal = $Subtotal,
    CreditCardFee = $CreditCardFee,
    Tax = $Tax,
    Total = $Total,

    Notes = $Notes,
    UpdatedAt = $UpdatedAt
WHERE QuoteId = $QuoteId;
";

                cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                cmd.Parameters.AddWithValue("$ClientId", quote.ClientId.ToString());
                cmd.Parameters.AddWithValue("$QuoteDate", quote.QuoteDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("$QuoteName", quote.QuoteName ?? "");
                cmd.Parameters.AddWithValue("$ServiceType", quote.ServiceType ?? "");
                cmd.Parameters.AddWithValue("$ServiceFrequency", quote.ServiceFrequency ?? "");
                cmd.Parameters.AddWithValue("$LastProfessionalCleaning", quote.LastProfessionalCleaning ?? "");
                cmd.Parameters.AddWithValue("$TotalSqFt", (double)quote.TotalSqFt);
                cmd.Parameters.AddWithValue("$UseTotalSqFtOverride", quote.UseTotalSqFtOverride ? 1 : 0);
                cmd.Parameters.AddWithValue("$EntryInstructions", quote.EntryInstructions ?? "");
                cmd.Parameters.AddWithValue("$PaymentMethod", quote.PaymentMethod ?? "");
                cmd.Parameters.AddWithValue("$PaymentMethodOther", quote.PaymentMethodOther ?? "");
                cmd.Parameters.AddWithValue("$FeedbackDiscussed", quote.FeedbackDiscussed ? 1 : 0);
                cmd.Parameters.AddWithValue("$Status", string.IsNullOrWhiteSpace(quote.Status) ? "Draft" : quote.Status);

                cmd.Parameters.AddWithValue("$LaborRate", (double)quote.LaborRate);
                cmd.Parameters.AddWithValue("$TaxRate", (double)quote.TaxRate);
                cmd.Parameters.AddWithValue("$CreditCardFeeRate", (double)quote.CreditCardFeeRate);
                cmd.Parameters.AddWithValue("$CreditCard", quote.CreditCard ? 1 : 0);

                cmd.Parameters.AddWithValue("$PetsCount", quote.PetsCount);
                cmd.Parameters.AddWithValue("$HouseholdSize", quote.HouseholdSize);
                cmd.Parameters.AddWithValue("$SmokingInside", quote.SmokingInside ? 1 : 0);

                cmd.Parameters.AddWithValue("$TotalLaborHours", (double)quote.TotalLaborHours);
                cmd.Parameters.AddWithValue("$Subtotal", (double)quote.Subtotal);
                cmd.Parameters.AddWithValue("$CreditCardFee", (double)quote.CreditCardFee);
                cmd.Parameters.AddWithValue("$Tax", (double)quote.Tax);
                cmd.Parameters.AddWithValue("$Total", (double)quote.Total);

                cmd.Parameters.AddWithValue("$Notes", quote.Notes ?? "");
                cmd.Parameters.AddWithValue("$UpdatedAt", now);

                var rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                    throw new InvalidOperationException("Update failed: QuoteId not found.");
            }

            // 2) Replace QuoteRooms (delete old, insert current)
            using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM QuoteRooms WHERE QuoteId = $QuoteId;";
                del.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                del.ExecuteNonQuery();
            }

            if (quote.Rooms != null)
            {
                foreach (var room in quote.Rooms)
                {
                    // Ensure each room has an id
                    if (room.QuoteRoomId == Guid.Empty)
                        room.QuoteRoomId = Guid.NewGuid();

                    using var ins = conn.CreateCommand();
                    ins.Transaction = tx;

                    ins.CommandText = @"
INSERT INTO QuoteRooms
(
    QuoteRoomId, QuoteId, ParentRoomId,
    RoomType, Size, Complexity,
    Level, ItemCategory, IsSubItem, IncludedInQuote, WindowInside, WindowOutside,
    FullGlassShowersCount, PebbleStoneFloorsCount, FridgeCount, OvenCount,
    RoomLaborHours, RoomAmount, RoomNotes
)
VALUES
(
    $QuoteRoomId, $QuoteId, $ParentRoomId,
    $RoomType, $Size, $Complexity,
    $Level, $ItemCategory, $IsSubItem, $IncludedInQuote, $WindowInside, $WindowOutside,
    $Glass, $Pebble, $Fridge, $Oven,
    $RoomHours, $RoomAmount, $RoomNotes
);
";
                    ins.Parameters.AddWithValue("$QuoteRoomId", room.QuoteRoomId.ToString());
                    ins.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                    ins.Parameters.AddWithValue("$ParentRoomId", (object?)room.ParentRoomId?.ToString() ?? DBNull.Value);
                    ins.Parameters.AddWithValue("$RoomType", room.RoomType ?? "");
                    ins.Parameters.AddWithValue("$Size", room.Size ?? "M");
                    ins.Parameters.AddWithValue("$Complexity", room.Complexity);
                    ins.Parameters.AddWithValue("$Level", room.Level ?? "");
                    ins.Parameters.AddWithValue("$ItemCategory", room.ItemCategory ?? "");
                    ins.Parameters.AddWithValue("$IsSubItem", room.IsSubItem ? 1 : 0);
                    ins.Parameters.AddWithValue("$IncludedInQuote", room.IncludedInQuote ? 1 : 0);
                    ins.Parameters.AddWithValue("$WindowInside", room.WindowInside ? 1 : 0);
                    ins.Parameters.AddWithValue("$WindowOutside", room.WindowOutside ? 1 : 0);
                    ins.Parameters.AddWithValue("$Glass", room.FullGlassShowersCount);
                    ins.Parameters.AddWithValue("$Pebble", room.PebbleStoneFloorsCount);
                    ins.Parameters.AddWithValue("$Fridge", room.FridgeCount);
                    ins.Parameters.AddWithValue("$Oven", room.OvenCount);
                    ins.Parameters.AddWithValue("$RoomHours", (double)room.RoomLaborHours);
                    ins.Parameters.AddWithValue("$RoomAmount", (double)room.RoomAmount);
                    ins.Parameters.AddWithValue("$RoomNotes", room.RoomNotes ?? "");

                    ins.ExecuteNonQuery();
                }
            }

            DeleteQuotePets(conn, tx, quote.QuoteId);
            InsertQuotePets(conn, tx, quote);

            DeleteQuoteOccupants(conn, tx, quote.QuoteId);
            InsertQuoteOccupants(conn, tx, quote);

            tx.Commit();
        }

        public void Save(Quote quote)
        {
            if (QuoteExists(quote.QuoteId))
                Update(quote);
            else
                Insert(quote);
        }

        private bool QuoteExists(Guid quoteId)
        {
            using var conn = Db.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM Quotes WHERE QuoteId = $QuoteId LIMIT 1;";
            cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
            return cmd.ExecuteScalar() != null;
        }


        public void Delete(Guid quoteId)
        {
            using var conn = Db.OpenConnection();
            using var tx = conn.BeginTransaction();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM QuoteRooms WHERE QuoteId = $QuoteId;";
                cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM QuotePets WHERE QuoteId = $QuoteId;";
                cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM QuoteOccupants WHERE QuoteId = $QuoteId;";
                cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM Quotes WHERE QuoteId = $QuoteId;";
                cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        private static void InsertQuotePets(Microsoft.Data.Sqlite.SqliteConnection conn, Microsoft.Data.Sqlite.SqliteTransaction tx, Quote quote)
        {
            if (quote?.Pets == null)
                return;

            foreach (var pet in quote.Pets)
            {
                if (pet.QuotePetId == Guid.Empty)
                    pet.QuotePetId = Guid.NewGuid();

                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO QuotePets (QuotePetId, QuoteId, Name, Type, Notes)
VALUES ($QuotePetId, $QuoteId, $Name, $Type, $Notes);
";
                cmd.Parameters.AddWithValue("$QuotePetId", pet.QuotePetId.ToString());
                cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                cmd.Parameters.AddWithValue("$Name", pet.Name ?? "");
                cmd.Parameters.AddWithValue("$Type", pet.Type ?? "");
                cmd.Parameters.AddWithValue("$Notes", pet.Notes ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertQuoteOccupants(Microsoft.Data.Sqlite.SqliteConnection conn, Microsoft.Data.Sqlite.SqliteTransaction tx, Quote quote)
        {
            if (quote?.Occupants == null)
                return;

            foreach (var occupant in quote.Occupants)
            {
                if (occupant.QuoteOccupantId == Guid.Empty)
                    occupant.QuoteOccupantId = Guid.NewGuid();

                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO QuoteOccupants (QuoteOccupantId, QuoteId, Name, Relationship, Notes)
VALUES ($QuoteOccupantId, $QuoteId, $Name, $Relationship, $Notes);
";
                cmd.Parameters.AddWithValue("$QuoteOccupantId", occupant.QuoteOccupantId.ToString());
                cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                cmd.Parameters.AddWithValue("$Name", occupant.Name ?? "");
                cmd.Parameters.AddWithValue("$Relationship", occupant.Relationship ?? "");
                cmd.Parameters.AddWithValue("$Notes", occupant.Notes ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        private static void DeleteQuotePets(Microsoft.Data.Sqlite.SqliteConnection conn, Microsoft.Data.Sqlite.SqliteTransaction tx, Guid quoteId)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM QuotePets WHERE QuoteId = $QuoteId;";
            cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
            cmd.ExecuteNonQuery();
        }

        private static void DeleteQuoteOccupants(Microsoft.Data.Sqlite.SqliteConnection conn, Microsoft.Data.Sqlite.SqliteTransaction tx, Guid quoteId)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM QuoteOccupants WHERE QuoteId = $QuoteId;";
            cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
            cmd.ExecuteNonQuery();
        }

        private static void LoadQuotePets(Microsoft.Data.Sqlite.SqliteConnection conn, Quote quote)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT QuotePetId, Name, Type, Notes
FROM QuotePets
WHERE QuoteId = $QuoteId;
";
            cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                quote.Pets.Add(new QuotePet
                {
                    QuotePetId = Guid.Parse(reader.GetString(0)),
                    QuoteId = quote.QuoteId,
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Notes = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }
        }

        private static void LoadQuoteOccupants(Microsoft.Data.Sqlite.SqliteConnection conn, Quote quote)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT QuoteOccupantId, Name, Relationship, Notes
FROM QuoteOccupants
WHERE QuoteId = $QuoteId;
";
            cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                quote.Occupants.Add(new QuoteOccupant
                {
                    QuoteOccupantId = Guid.Parse(reader.GetString(0)),
                    QuoteId = quote.QuoteId,
                    Name = reader.GetString(1),
                    Relationship = reader.GetString(2),
                    Notes = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }
        }
    }
}

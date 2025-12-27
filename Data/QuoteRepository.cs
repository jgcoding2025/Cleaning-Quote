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
(QuoteId, ClientId, QuoteDate, Status, LaborRate, TaxRate, CreditCardFeeRate, CreditCard,
 PetsCount, HouseholdSize, SmokingInside, TotalLaborHours, Subtotal, CreditCardFee, Tax, Total,
 Notes, CreatedAt, UpdatedAt)
VALUES
($QuoteId, $ClientId, $QuoteDate, $Status, $LaborRate, $TaxRate, $CreditCardFeeRate, $CreditCard,
 $PetsCount, $HouseholdSize, $SmokingInside, $TotalLaborHours, $Subtotal, $CreditCardFee, $Tax, $Total,
 $Notes, $CreatedAt, $UpdatedAt);
";

                cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                cmd.Parameters.AddWithValue("$ClientId", quote.ClientId.ToString());
                cmd.Parameters.AddWithValue("$QuoteDate", quote.QuoteDate.ToString("yyyy-MM-dd"));
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
    QuoteRoomId, QuoteId,
    RoomType, Size, Complexity,
    FullGlassShowersCount, PebbleStoneFloorsCount, FridgeCount, OvenCount,
    RoomLaborHours
)
VALUES
(
    $QuoteRoomId, $QuoteId,
    $RoomType, $Size, $Complexity,
    $Glass, $Pebble, $Fridge, $Oven,
    $RoomHours
);
";
                    roomCmd.Parameters.AddWithValue("$QuoteRoomId", room.QuoteRoomId.ToString());
                    roomCmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                    roomCmd.Parameters.AddWithValue("$RoomType", room.RoomType ?? "");
                    roomCmd.Parameters.AddWithValue("$Size", room.Size ?? "M");
                    roomCmd.Parameters.AddWithValue("$Complexity", room.Complexity);
                    roomCmd.Parameters.AddWithValue("$Glass", room.FullGlassShowersCount);
                    roomCmd.Parameters.AddWithValue("$Pebble", room.PebbleStoneFloorsCount);
                    roomCmd.Parameters.AddWithValue("$Fridge", room.FridgeCount);
                    roomCmd.Parameters.AddWithValue("$Oven", room.OvenCount);
                    roomCmd.Parameters.AddWithValue("$RoomHours", (double)room.RoomLaborHours);

                    roomCmd.ExecuteNonQuery();
                }
            }

            tx.Commit();
        }

        public List<QuoteListItem> GetForClient(Guid clientId)
        {
            var list = new List<QuoteListItem>();

            using var conn = Db.OpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
SELECT QuoteId, QuoteDate, Total, TotalLaborHours
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
                SELECT QuoteId, ClientId, QuoteDate,
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

                    LaborRate = Convert.ToDecimal(r.GetDouble(3)),
                    TaxRate = Convert.ToDecimal(r.GetDouble(4)),
                    CreditCardFeeRate = Convert.ToDecimal(r.GetDouble(5)),
                    CreditCard = r.GetInt32(6) == 1,

                    PetsCount = r.GetInt32(7),
                    HouseholdSize = r.GetInt32(8),
                    SmokingInside = r.GetInt32(9) == 1,

                    TotalLaborHours = Convert.ToDecimal(r.GetDouble(10)),
                    Subtotal = Convert.ToDecimal(r.GetDouble(11)),
                    CreditCardFee = Convert.ToDecimal(r.GetDouble(12)),
                    Tax = Convert.ToDecimal(r.GetDouble(13)),
                    Total = Convert.ToDecimal(r.GetDouble(14)),

                    Notes = r.IsDBNull(15) ? "" : r.GetString(15),
                };
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT QuoteRoomId, RoomType, Size, Complexity,
       FullGlassShowersCount, PebbleStoneFloorsCount, FridgeCount, OvenCount,
       RoomLaborHours
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
                        FullGlassShowersCount = r.GetInt32(4),
                        PebbleStoneFloorsCount = r.GetInt32(5),
                        FridgeCount = r.GetInt32(6),
                        OvenCount = r.GetInt32(7),
                        RoomLaborHours = Convert.ToDecimal(r.GetDouble(8)),
                    });
                }
            }

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
    QuoteRoomId, QuoteId,
    RoomType, Size, Complexity,
    FullGlassShowersCount, PebbleStoneFloorsCount, FridgeCount, OvenCount,
    RoomLaborHours
)
VALUES
(
    $QuoteRoomId, $QuoteId,
    $RoomType, $Size, $Complexity,
    $Glass, $Pebble, $Fridge, $Oven,
    $RoomHours
);
";
                    ins.Parameters.AddWithValue("$QuoteRoomId", room.QuoteRoomId.ToString());
                    ins.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
                    ins.Parameters.AddWithValue("$RoomType", room.RoomType ?? "");
                    ins.Parameters.AddWithValue("$Size", room.Size ?? "M");
                    ins.Parameters.AddWithValue("$Complexity", room.Complexity);
                    ins.Parameters.AddWithValue("$Glass", room.FullGlassShowersCount);
                    ins.Parameters.AddWithValue("$Pebble", room.PebbleStoneFloorsCount);
                    ins.Parameters.AddWithValue("$Fridge", room.FridgeCount);
                    ins.Parameters.AddWithValue("$Oven", room.OvenCount);
                    ins.Parameters.AddWithValue("$RoomHours", (double)room.RoomLaborHours);

                    ins.ExecuteNonQuery();
                }
            }

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
                cmd.CommandText = "DELETE FROM Quotes WHERE QuoteId = $QuoteId;";
                cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
    }
}

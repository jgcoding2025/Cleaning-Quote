using CleaningQuote.Api.Models;
using Microsoft.Data.Sqlite;

namespace CleaningQuote.Api.Data;

public sealed class QuoteRepository
{
    private readonly ApiDb _db;

    public QuoteRepository(ApiDb db)
    {
        _db = db;
    }

    public List<QuoteSummary> GetSummaries()
    {
        var list = new List<QuoteSummary>();

        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT QuoteId, QuoteDate, Total, Status, QuoteName
FROM Quotes
ORDER BY QuoteDate DESC, CreatedAt DESC;
";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new QuoteSummary(
                Guid.Parse(reader.GetString(0)),
                reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                DateTime.Parse(reader.GetString(1)),
                Convert.ToDecimal(reader.GetDouble(2)),
                reader.IsDBNull(3) ? "Draft" : reader.GetString(3)));
        }

        return list;
    }

    public QuoteDetail? GetById(Guid quoteId)
    {
        using var connection = _db.OpenConnection();

        QuoteDetail? quote = null;
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
SELECT QuoteId, ClientId, QuoteDate, QuoteName, ServiceType, ServiceFrequency, LastProfessionalCleaning,
       TotalSqFt, UseTotalSqFtOverride, EntryInstructions, PaymentMethod, PaymentMethodOther, FeedbackDiscussed,
       Status, LaborRate, TaxRate, CreditCardFeeRate, CreditCard,
       PetsCount, HouseholdSize, SmokingInside,
       TotalLaborHours, Subtotal, CreditCardFee, Tax, Total,
       Notes
FROM Quotes
WHERE QuoteId = $QuoteId;
";
            cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            quote = new QuoteDetail
            {
                QuoteId = Guid.Parse(reader.GetString(0)),
                ClientId = Guid.Parse(reader.GetString(1)),
                QuoteDate = DateTime.Parse(reader.GetString(2)),
                QuoteName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                ServiceType = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                ServiceFrequency = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                LastProfessionalCleaning = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                TotalSqFt = reader.IsDBNull(7) ? 0m : Convert.ToDecimal(reader.GetDouble(7)),
                UseTotalSqFtOverride = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                EntryInstructions = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                PaymentMethod = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                PaymentMethodOther = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                FeedbackDiscussed = !reader.IsDBNull(12) && reader.GetInt32(12) == 1,
                Status = reader.IsDBNull(13) ? "Draft" : reader.GetString(13),
                LaborRate = Convert.ToDecimal(reader.GetDouble(14)),
                TaxRate = Convert.ToDecimal(reader.GetDouble(15)),
                CreditCardFeeRate = Convert.ToDecimal(reader.GetDouble(16)),
                CreditCard = reader.GetInt32(17) == 1,
                PetsCount = reader.IsDBNull(18) ? 0 : reader.GetInt32(18),
                HouseholdSize = reader.IsDBNull(19) ? 2 : reader.GetInt32(19),
                SmokingInside = !reader.IsDBNull(20) && reader.GetInt32(20) == 1,
                TotalLaborHours = Convert.ToDecimal(reader.GetDouble(21)),
                Subtotal = Convert.ToDecimal(reader.GetDouble(22)),
                CreditCardFee = Convert.ToDecimal(reader.GetDouble(23)),
                Tax = Convert.ToDecimal(reader.GetDouble(24)),
                Total = Convert.ToDecimal(reader.GetDouble(25)),
                Notes = reader.IsDBNull(26) ? string.Empty : reader.GetString(26)
            };
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
SELECT QuoteRoomId, RoomType, Size, Complexity,
       Level, ItemCategory, IsSubItem, ParentRoomId, IncludedInQuote,
       RoomLaborHours, RoomAmount, RoomNotes, SortOrder
FROM QuoteRooms
WHERE QuoteId = $QuoteId
ORDER BY SortOrder ASC, rowid ASC;
";
            cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                quote.Rooms.Add(new QuoteRoomDetail
                {
                    QuoteRoomId = Guid.Parse(reader.GetString(0)),
                    QuoteId = quoteId,
                    RoomType = reader.GetString(1),
                    Size = reader.GetString(2),
                    Complexity = reader.GetInt32(3),
                    Level = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    ItemCategory = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    IsSubItem = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
                    ParentRoomId = reader.IsDBNull(7) ? null : Guid.Parse(reader.GetString(7)),
                    IncludedInQuote = reader.IsDBNull(8) || reader.GetInt32(8) == 1,
                    RoomLaborHours = reader.IsDBNull(9) ? 0m : Convert.ToDecimal(reader.GetDouble(9)),
                    RoomAmount = reader.IsDBNull(10) ? 0m : Convert.ToDecimal(reader.GetDouble(10)),
                    RoomNotes = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    SortOrder = reader.IsDBNull(12) ? 0 : reader.GetInt32(12)
                });
            }
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
SELECT QuotePetId, Name, Type, Notes
FROM QuotePets
WHERE QuoteId = $QuoteId
ORDER BY rowid ASC;
";
            cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                quote.Pets.Add(new QuotePetDetail
                {
                    QuotePetId = Guid.Parse(reader.GetString(0)),
                    QuoteId = quoteId,
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Notes = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                });
            }
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
SELECT QuoteOccupantId, Name, Relationship, Notes
FROM QuoteOccupants
WHERE QuoteId = $QuoteId
ORDER BY rowid ASC;
";
            cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                quote.Occupants.Add(new QuoteOccupantDetail
                {
                    QuoteOccupantId = Guid.Parse(reader.GetString(0)),
                    QuoteId = quoteId,
                    Name = reader.GetString(1),
                    Relationship = reader.GetString(2),
                    Notes = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                });
            }
        }

        return quote;
    }

    public bool Update(QuoteDetail quote)
    {
        using var connection = _db.OpenConnection();
        using var transaction = connection.BeginTransaction();

        var now = DateTime.UtcNow.ToString("o");

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = @"
UPDATE Quotes
SET ClientId = $ClientId,
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
            cmd.Parameters.AddWithValue("$QuoteName", quote.QuoteName ?? string.Empty);
            cmd.Parameters.AddWithValue("$ServiceType", quote.ServiceType ?? string.Empty);
            cmd.Parameters.AddWithValue("$ServiceFrequency", quote.ServiceFrequency ?? string.Empty);
            cmd.Parameters.AddWithValue("$LastProfessionalCleaning", quote.LastProfessionalCleaning ?? string.Empty);
            cmd.Parameters.AddWithValue("$TotalSqFt", (double)quote.TotalSqFt);
            cmd.Parameters.AddWithValue("$UseTotalSqFtOverride", quote.UseTotalSqFtOverride ? 1 : 0);
            cmd.Parameters.AddWithValue("$EntryInstructions", quote.EntryInstructions ?? string.Empty);
            cmd.Parameters.AddWithValue("$PaymentMethod", quote.PaymentMethod ?? string.Empty);
            cmd.Parameters.AddWithValue("$PaymentMethodOther", quote.PaymentMethodOther ?? string.Empty);
            cmd.Parameters.AddWithValue("$FeedbackDiscussed", quote.FeedbackDiscussed ? 1 : 0);
            cmd.Parameters.AddWithValue("$Status", quote.Status ?? "Draft");
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
            cmd.Parameters.AddWithValue("$Notes", quote.Notes ?? string.Empty);
            cmd.Parameters.AddWithValue("$UpdatedAt", now);

            if (cmd.ExecuteNonQuery() == 0)
            {
                transaction.Rollback();
                return false;
            }
        }

        DeleteChildren(connection, transaction, quote.QuoteId);
        InsertRooms(connection, transaction, quote);
        InsertPets(connection, transaction, quote);
        InsertOccupants(connection, transaction, quote);

        transaction.Commit();
        return true;
    }

    private static void DeleteChildren(SqliteConnection connection, SqliteTransaction transaction, Guid quoteId)
    {
        foreach (var table in new[] { "QuoteRooms", "QuotePets", "QuoteOccupants" })
        {
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"DELETE FROM {table} WHERE QuoteId = $QuoteId;";
            cmd.Parameters.AddWithValue("$QuoteId", quoteId.ToString());
            cmd.ExecuteNonQuery();
        }
    }

    private static void InsertRooms(SqliteConnection connection, SqliteTransaction transaction, QuoteDetail quote)
    {
        foreach (var room in quote.Rooms)
        {
            var roomId = room.QuoteRoomId == Guid.Empty ? Guid.NewGuid() : room.QuoteRoomId;
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
INSERT INTO QuoteRooms
(
    QuoteRoomId, QuoteId, ParentRoomId,
    RoomType, Size, Complexity,
    Level, ItemCategory, IsSubItem, IncludedInQuote,
    RoomLaborHours, RoomAmount, RoomNotes, SortOrder
)
VALUES
(
    $QuoteRoomId, $QuoteId, $ParentRoomId,
    $RoomType, $Size, $Complexity,
    $Level, $ItemCategory, $IsSubItem, $IncludedInQuote,
    $RoomHours, $RoomAmount, $RoomNotes, $SortOrder
);
";
            cmd.Parameters.AddWithValue("$QuoteRoomId", roomId.ToString());
            cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
            cmd.Parameters.AddWithValue("$ParentRoomId", (object?)room.ParentRoomId?.ToString() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$RoomType", room.RoomType ?? string.Empty);
            cmd.Parameters.AddWithValue("$Size", room.Size ?? "M");
            cmd.Parameters.AddWithValue("$Complexity", room.Complexity);
            cmd.Parameters.AddWithValue("$Level", room.Level ?? string.Empty);
            cmd.Parameters.AddWithValue("$ItemCategory", room.ItemCategory ?? string.Empty);
            cmd.Parameters.AddWithValue("$IsSubItem", room.IsSubItem ? 1 : 0);
            cmd.Parameters.AddWithValue("$IncludedInQuote", room.IncludedInQuote ? 1 : 0);
            cmd.Parameters.AddWithValue("$RoomHours", (double)room.RoomLaborHours);
            cmd.Parameters.AddWithValue("$RoomAmount", (double)room.RoomAmount);
            cmd.Parameters.AddWithValue("$RoomNotes", room.RoomNotes ?? string.Empty);
            cmd.Parameters.AddWithValue("$SortOrder", room.SortOrder);

            cmd.ExecuteNonQuery();
        }
    }

    private static void InsertPets(SqliteConnection connection, SqliteTransaction transaction, QuoteDetail quote)
    {
        foreach (var pet in quote.Pets)
        {
            var petId = pet.QuotePetId == Guid.Empty ? Guid.NewGuid() : pet.QuotePetId;
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
INSERT INTO QuotePets (QuotePetId, QuoteId, Name, Type, Notes)
VALUES ($QuotePetId, $QuoteId, $Name, $Type, $Notes);
";
            cmd.Parameters.AddWithValue("$QuotePetId", petId.ToString());
            cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
            cmd.Parameters.AddWithValue("$Name", pet.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("$Type", pet.Type ?? string.Empty);
            cmd.Parameters.AddWithValue("$Notes", pet.Notes ?? string.Empty);
            cmd.ExecuteNonQuery();
        }
    }

    private static void InsertOccupants(SqliteConnection connection, SqliteTransaction transaction, QuoteDetail quote)
    {
        foreach (var occupant in quote.Occupants)
        {
            var occupantId = occupant.QuoteOccupantId == Guid.Empty ? Guid.NewGuid() : occupant.QuoteOccupantId;
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
INSERT INTO QuoteOccupants (QuoteOccupantId, QuoteId, Name, Relationship, Notes)
VALUES ($QuoteOccupantId, $QuoteId, $Name, $Relationship, $Notes);
";
            cmd.Parameters.AddWithValue("$QuoteOccupantId", occupantId.ToString());
            cmd.Parameters.AddWithValue("$QuoteId", quote.QuoteId.ToString());
            cmd.Parameters.AddWithValue("$Name", occupant.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("$Relationship", occupant.Relationship ?? string.Empty);
            cmd.Parameters.AddWithValue("$Notes", occupant.Notes ?? string.Empty);
            cmd.ExecuteNonQuery();
        }
    }
}

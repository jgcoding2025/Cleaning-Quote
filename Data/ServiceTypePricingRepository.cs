using Cleaning_Quote.Models;
using System;

namespace Cleaning_Quote.Data
{
    public class ServiceTypePricingRepository
    {
        public ServiceTypePricing GetOrCreate(string serviceType)
        {
            var normalized = serviceType?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(normalized))
                return ServiceTypePricing.Default(normalized);

            using var conn = Db.OpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
SELECT ServiceType, SqFtPerLaborHour, SizeSmallSqFt, SizeMediumSqFt, SizeLargeSqFt,
       Complexity1Multiplier, Complexity2Multiplier, Complexity3Multiplier,
       Complexity1Definition, Complexity2Definition, Complexity3Definition,
       FullGlassShowerHoursEach, PebbleStoneFloorHoursEach, FridgeHoursEach, OvenHoursEach,
       UpdatedAt
FROM ServiceTypePricing
WHERE ServiceType = $ServiceType;
";
            cmd.Parameters.AddWithValue("$ServiceType", normalized);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new ServiceTypePricing
                {
                    ServiceType = reader.GetString(0),
                    SqFtPerLaborHour = Convert.ToDecimal(reader.GetDouble(1)),
                    SizeSmallSqFt = Convert.ToDecimal(reader.GetDouble(2)),
                    SizeMediumSqFt = Convert.ToDecimal(reader.GetDouble(3)),
                    SizeLargeSqFt = Convert.ToDecimal(reader.GetDouble(4)),
                    Complexity1Multiplier = Convert.ToDecimal(reader.GetDouble(5)),
                    Complexity2Multiplier = Convert.ToDecimal(reader.GetDouble(6)),
                    Complexity3Multiplier = Convert.ToDecimal(reader.GetDouble(7)),
                    Complexity1Definition = GetOptionalString(reader, 8),
                    Complexity2Definition = GetOptionalString(reader, 9),
                    Complexity3Definition = GetOptionalString(reader, 10),
                    FullGlassShowerHoursEach = Convert.ToDecimal(reader.GetDouble(11)),
                    PebbleStoneFloorHoursEach = Convert.ToDecimal(reader.GetDouble(12)),
                    FridgeHoursEach = Convert.ToDecimal(reader.GetDouble(13)),
                    OvenHoursEach = Convert.ToDecimal(reader.GetDouble(14)),
                    UpdatedAt = DateTime.Parse(reader.GetString(15))
                };
            }

            var defaults = ServiceTypePricing.Default(normalized);
            Upsert(defaults);
            return defaults;
        }

        public void Upsert(ServiceTypePricing settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var normalized = settings.ServiceType?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(normalized))
                return;

            settings.ServiceType = normalized;
            settings.UpdatedAt = DateTime.UtcNow;

            using var conn = Db.OpenConnection();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE ServiceTypePricing
SET SqFtPerLaborHour = $SqFtPerLaborHour,
    SizeSmallSqFt = $SizeSmallSqFt,
    SizeMediumSqFt = $SizeMediumSqFt,
    SizeLargeSqFt = $SizeLargeSqFt,
    Complexity1Multiplier = $Complexity1Multiplier,
    Complexity2Multiplier = $Complexity2Multiplier,
    Complexity3Multiplier = $Complexity3Multiplier,
    Complexity1Definition = $Complexity1Definition,
    Complexity2Definition = $Complexity2Definition,
    Complexity3Definition = $Complexity3Definition,
    FullGlassShowerHoursEach = $FullGlassShowerHoursEach,
    PebbleStoneFloorHoursEach = $PebbleStoneFloorHoursEach,
    FridgeHoursEach = $FridgeHoursEach,
    OvenHoursEach = $OvenHoursEach,
    UpdatedAt = $UpdatedAt
WHERE ServiceType = $ServiceType;
";
                cmd.Parameters.AddWithValue("$ServiceType", normalized);
                cmd.Parameters.AddWithValue("$SqFtPerLaborHour", (double)settings.SqFtPerLaborHour);
                cmd.Parameters.AddWithValue("$SizeSmallSqFt", (double)settings.SizeSmallSqFt);
                cmd.Parameters.AddWithValue("$SizeMediumSqFt", (double)settings.SizeMediumSqFt);
                cmd.Parameters.AddWithValue("$SizeLargeSqFt", (double)settings.SizeLargeSqFt);
                cmd.Parameters.AddWithValue("$Complexity1Multiplier", (double)settings.Complexity1Multiplier);
                cmd.Parameters.AddWithValue("$Complexity2Multiplier", (double)settings.Complexity2Multiplier);
                cmd.Parameters.AddWithValue("$Complexity3Multiplier", (double)settings.Complexity3Multiplier);
                cmd.Parameters.AddWithValue("$Complexity1Definition", settings.Complexity1Definition ?? "");
                cmd.Parameters.AddWithValue("$Complexity2Definition", settings.Complexity2Definition ?? "");
                cmd.Parameters.AddWithValue("$Complexity3Definition", settings.Complexity3Definition ?? "");
                cmd.Parameters.AddWithValue("$FullGlassShowerHoursEach", (double)settings.FullGlassShowerHoursEach);
                cmd.Parameters.AddWithValue("$PebbleStoneFloorHoursEach", (double)settings.PebbleStoneFloorHoursEach);
                cmd.Parameters.AddWithValue("$FridgeHoursEach", (double)settings.FridgeHoursEach);
                cmd.Parameters.AddWithValue("$OvenHoursEach", (double)settings.OvenHoursEach);
                cmd.Parameters.AddWithValue("$UpdatedAt", settings.UpdatedAt.ToString("o"));

                var rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return;
            }

            using (var insert = conn.CreateCommand())
            {
                insert.CommandText = @"
INSERT INTO ServiceTypePricing
(ServiceType, SqFtPerLaborHour, SizeSmallSqFt, SizeMediumSqFt, SizeLargeSqFt,
 Complexity1Multiplier, Complexity2Multiplier, Complexity3Multiplier,
 Complexity1Definition, Complexity2Definition, Complexity3Definition,
 FullGlassShowerHoursEach, PebbleStoneFloorHoursEach, FridgeHoursEach, OvenHoursEach,
 UpdatedAt)
VALUES
($ServiceType, $SqFtPerLaborHour, $SizeSmallSqFt, $SizeMediumSqFt, $SizeLargeSqFt,
 $Complexity1Multiplier, $Complexity2Multiplier, $Complexity3Multiplier,
 $Complexity1Definition, $Complexity2Definition, $Complexity3Definition,
 $FullGlassShowerHoursEach, $PebbleStoneFloorHoursEach, $FridgeHoursEach, $OvenHoursEach,
 $UpdatedAt);
";
                insert.Parameters.AddWithValue("$ServiceType", normalized);
                insert.Parameters.AddWithValue("$SqFtPerLaborHour", (double)settings.SqFtPerLaborHour);
                insert.Parameters.AddWithValue("$SizeSmallSqFt", (double)settings.SizeSmallSqFt);
                insert.Parameters.AddWithValue("$SizeMediumSqFt", (double)settings.SizeMediumSqFt);
                insert.Parameters.AddWithValue("$SizeLargeSqFt", (double)settings.SizeLargeSqFt);
                insert.Parameters.AddWithValue("$Complexity1Multiplier", (double)settings.Complexity1Multiplier);
                insert.Parameters.AddWithValue("$Complexity2Multiplier", (double)settings.Complexity2Multiplier);
                insert.Parameters.AddWithValue("$Complexity3Multiplier", (double)settings.Complexity3Multiplier);
                insert.Parameters.AddWithValue("$Complexity1Definition", settings.Complexity1Definition ?? "");
                insert.Parameters.AddWithValue("$Complexity2Definition", settings.Complexity2Definition ?? "");
                insert.Parameters.AddWithValue("$Complexity3Definition", settings.Complexity3Definition ?? "");
                insert.Parameters.AddWithValue("$FullGlassShowerHoursEach", (double)settings.FullGlassShowerHoursEach);
                insert.Parameters.AddWithValue("$PebbleStoneFloorHoursEach", (double)settings.PebbleStoneFloorHoursEach);
                insert.Parameters.AddWithValue("$FridgeHoursEach", (double)settings.FridgeHoursEach);
                insert.Parameters.AddWithValue("$OvenHoursEach", (double)settings.OvenHoursEach);
                insert.Parameters.AddWithValue("$UpdatedAt", settings.UpdatedAt.ToString("o"));

                insert.ExecuteNonQuery();
            }
        }

        private static string GetOptionalString(System.Data.Common.DbDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? "" : reader.GetString(index);
        }
    }
}

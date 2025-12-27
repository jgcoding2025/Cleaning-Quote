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
       FullGlassShowerHoursEach, FullGlassShowerComplexity,
       PebbleStoneFloorHoursEach, PebbleStoneFloorComplexity,
       FridgeHoursEach, FridgeComplexity,
       OvenHoursEach, OvenComplexity,
       CeilingFanHoursEach, CeilingFanComplexity,
       WindowSmallHoursEach, WindowMediumHoursEach, WindowLargeHoursEach, WindowComplexity,
       FirstCleanRate, FirstCleanMinimum,
       DeepCleanRate, DeepCleanMinimum,
       MaintenanceRate, MaintenanceMinimum,
       OneTimeDeepCleanRate, OneTimeDeepCleanMinimum,
       WindowInsideRate, WindowOutsideRate,
       UpdatedAt
FROM ServiceTypePricing
WHERE ServiceType = $ServiceType;
";
            cmd.Parameters.AddWithValue("$ServiceType", normalized);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var defaults = ServiceTypePricing.Default(normalized);
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
                    FullGlassShowerComplexity = GetOptionalInt(reader, 12, defaults.FullGlassShowerComplexity),
                    PebbleStoneFloorHoursEach = Convert.ToDecimal(reader.GetDouble(13)),
                    PebbleStoneFloorComplexity = GetOptionalInt(reader, 14, defaults.PebbleStoneFloorComplexity),
                    FridgeHoursEach = Convert.ToDecimal(reader.GetDouble(15)),
                    FridgeComplexity = GetOptionalInt(reader, 16, defaults.FridgeComplexity),
                    OvenHoursEach = Convert.ToDecimal(reader.GetDouble(17)),
                    OvenComplexity = GetOptionalInt(reader, 18, defaults.OvenComplexity),
                    CeilingFanHoursEach = GetOptionalDecimal(reader, 19, defaults.CeilingFanHoursEach),
                    CeilingFanComplexity = GetOptionalInt(reader, 20, defaults.CeilingFanComplexity),
                    WindowSmallHoursEach = GetOptionalDecimal(reader, 21, defaults.WindowSmallHoursEach),
                    WindowMediumHoursEach = GetOptionalDecimal(reader, 22, defaults.WindowMediumHoursEach),
                    WindowLargeHoursEach = GetOptionalDecimal(reader, 23, defaults.WindowLargeHoursEach),
                    WindowComplexity = GetOptionalInt(reader, 24, defaults.WindowComplexity),
                    FirstCleanRate = GetOptionalDecimal(reader, 25, defaults.FirstCleanRate),
                    FirstCleanMinimum = GetOptionalDecimal(reader, 26, defaults.FirstCleanMinimum),
                    DeepCleanRate = GetOptionalDecimal(reader, 27, defaults.DeepCleanRate),
                    DeepCleanMinimum = GetOptionalDecimal(reader, 28, defaults.DeepCleanMinimum),
                    MaintenanceRate = GetOptionalDecimal(reader, 29, defaults.MaintenanceRate),
                    MaintenanceMinimum = GetOptionalDecimal(reader, 30, defaults.MaintenanceMinimum),
                    OneTimeDeepCleanRate = GetOptionalDecimal(reader, 31, defaults.OneTimeDeepCleanRate),
                    OneTimeDeepCleanMinimum = GetOptionalDecimal(reader, 32, defaults.OneTimeDeepCleanMinimum),
                    WindowInsideRate = GetOptionalDecimal(reader, 33, defaults.WindowInsideRate),
                    WindowOutsideRate = GetOptionalDecimal(reader, 34, defaults.WindowOutsideRate),
                    UpdatedAt = DateTime.Parse(reader.GetString(35))
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
    FullGlassShowerComplexity = $FullGlassShowerComplexity,
    PebbleStoneFloorHoursEach = $PebbleStoneFloorHoursEach,
    PebbleStoneFloorComplexity = $PebbleStoneFloorComplexity,
    FridgeHoursEach = $FridgeHoursEach,
    FridgeComplexity = $FridgeComplexity,
    OvenHoursEach = $OvenHoursEach,
    OvenComplexity = $OvenComplexity,
    CeilingFanHoursEach = $CeilingFanHoursEach,
    CeilingFanComplexity = $CeilingFanComplexity,
    WindowSmallHoursEach = $WindowSmallHoursEach,
    WindowMediumHoursEach = $WindowMediumHoursEach,
    WindowLargeHoursEach = $WindowLargeHoursEach,
    WindowComplexity = $WindowComplexity,
    FirstCleanRate = $FirstCleanRate,
    FirstCleanMinimum = $FirstCleanMinimum,
    DeepCleanRate = $DeepCleanRate,
    DeepCleanMinimum = $DeepCleanMinimum,
    MaintenanceRate = $MaintenanceRate,
    MaintenanceMinimum = $MaintenanceMinimum,
    OneTimeDeepCleanRate = $OneTimeDeepCleanRate,
    OneTimeDeepCleanMinimum = $OneTimeDeepCleanMinimum,
    WindowInsideRate = $WindowInsideRate,
    WindowOutsideRate = $WindowOutsideRate,
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
                cmd.Parameters.AddWithValue("$FullGlassShowerComplexity", settings.FullGlassShowerComplexity);
                cmd.Parameters.AddWithValue("$PebbleStoneFloorHoursEach", (double)settings.PebbleStoneFloorHoursEach);
                cmd.Parameters.AddWithValue("$PebbleStoneFloorComplexity", settings.PebbleStoneFloorComplexity);
                cmd.Parameters.AddWithValue("$FridgeHoursEach", (double)settings.FridgeHoursEach);
                cmd.Parameters.AddWithValue("$FridgeComplexity", settings.FridgeComplexity);
                cmd.Parameters.AddWithValue("$OvenHoursEach", (double)settings.OvenHoursEach);
                cmd.Parameters.AddWithValue("$OvenComplexity", settings.OvenComplexity);
                cmd.Parameters.AddWithValue("$CeilingFanHoursEach", (double)settings.CeilingFanHoursEach);
                cmd.Parameters.AddWithValue("$CeilingFanComplexity", settings.CeilingFanComplexity);
                cmd.Parameters.AddWithValue("$WindowSmallHoursEach", (double)settings.WindowSmallHoursEach);
                cmd.Parameters.AddWithValue("$WindowMediumHoursEach", (double)settings.WindowMediumHoursEach);
                cmd.Parameters.AddWithValue("$WindowLargeHoursEach", (double)settings.WindowLargeHoursEach);
                cmd.Parameters.AddWithValue("$WindowComplexity", settings.WindowComplexity);
                cmd.Parameters.AddWithValue("$FirstCleanRate", (double)settings.FirstCleanRate);
                cmd.Parameters.AddWithValue("$FirstCleanMinimum", (double)settings.FirstCleanMinimum);
                cmd.Parameters.AddWithValue("$DeepCleanRate", (double)settings.DeepCleanRate);
                cmd.Parameters.AddWithValue("$DeepCleanMinimum", (double)settings.DeepCleanMinimum);
                cmd.Parameters.AddWithValue("$MaintenanceRate", (double)settings.MaintenanceRate);
                cmd.Parameters.AddWithValue("$MaintenanceMinimum", (double)settings.MaintenanceMinimum);
                cmd.Parameters.AddWithValue("$OneTimeDeepCleanRate", (double)settings.OneTimeDeepCleanRate);
                cmd.Parameters.AddWithValue("$OneTimeDeepCleanMinimum", (double)settings.OneTimeDeepCleanMinimum);
                cmd.Parameters.AddWithValue("$WindowInsideRate", (double)settings.WindowInsideRate);
                cmd.Parameters.AddWithValue("$WindowOutsideRate", (double)settings.WindowOutsideRate);
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
 FullGlassShowerHoursEach, FullGlassShowerComplexity,
 PebbleStoneFloorHoursEach, PebbleStoneFloorComplexity,
 FridgeHoursEach, FridgeComplexity,
 OvenHoursEach, OvenComplexity,
 CeilingFanHoursEach, CeilingFanComplexity,
 WindowSmallHoursEach, WindowMediumHoursEach, WindowLargeHoursEach, WindowComplexity,
 FirstCleanRate, FirstCleanMinimum,
 DeepCleanRate, DeepCleanMinimum,
 MaintenanceRate, MaintenanceMinimum,
 OneTimeDeepCleanRate, OneTimeDeepCleanMinimum,
 WindowInsideRate, WindowOutsideRate,
 UpdatedAt)
VALUES
($ServiceType, $SqFtPerLaborHour, $SizeSmallSqFt, $SizeMediumSqFt, $SizeLargeSqFt,
 $Complexity1Multiplier, $Complexity2Multiplier, $Complexity3Multiplier,
 $Complexity1Definition, $Complexity2Definition, $Complexity3Definition,
 $FullGlassShowerHoursEach, $FullGlassShowerComplexity,
 $PebbleStoneFloorHoursEach, $PebbleStoneFloorComplexity,
 $FridgeHoursEach, $FridgeComplexity,
 $OvenHoursEach, $OvenComplexity,
 $CeilingFanHoursEach, $CeilingFanComplexity,
 $WindowSmallHoursEach, $WindowMediumHoursEach, $WindowLargeHoursEach, $WindowComplexity,
 $FirstCleanRate, $FirstCleanMinimum,
 $DeepCleanRate, $DeepCleanMinimum,
 $MaintenanceRate, $MaintenanceMinimum,
 $OneTimeDeepCleanRate, $OneTimeDeepCleanMinimum,
 $WindowInsideRate, $WindowOutsideRate,
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
                insert.Parameters.AddWithValue("$FullGlassShowerComplexity", settings.FullGlassShowerComplexity);
                insert.Parameters.AddWithValue("$PebbleStoneFloorHoursEach", (double)settings.PebbleStoneFloorHoursEach);
                insert.Parameters.AddWithValue("$PebbleStoneFloorComplexity", settings.PebbleStoneFloorComplexity);
                insert.Parameters.AddWithValue("$FridgeHoursEach", (double)settings.FridgeHoursEach);
                insert.Parameters.AddWithValue("$FridgeComplexity", settings.FridgeComplexity);
                insert.Parameters.AddWithValue("$OvenHoursEach", (double)settings.OvenHoursEach);
                insert.Parameters.AddWithValue("$OvenComplexity", settings.OvenComplexity);
                insert.Parameters.AddWithValue("$CeilingFanHoursEach", (double)settings.CeilingFanHoursEach);
                insert.Parameters.AddWithValue("$CeilingFanComplexity", settings.CeilingFanComplexity);
                insert.Parameters.AddWithValue("$WindowSmallHoursEach", (double)settings.WindowSmallHoursEach);
                insert.Parameters.AddWithValue("$WindowMediumHoursEach", (double)settings.WindowMediumHoursEach);
                insert.Parameters.AddWithValue("$WindowLargeHoursEach", (double)settings.WindowLargeHoursEach);
                insert.Parameters.AddWithValue("$WindowComplexity", settings.WindowComplexity);
                insert.Parameters.AddWithValue("$FirstCleanRate", (double)settings.FirstCleanRate);
                insert.Parameters.AddWithValue("$FirstCleanMinimum", (double)settings.FirstCleanMinimum);
                insert.Parameters.AddWithValue("$DeepCleanRate", (double)settings.DeepCleanRate);
                insert.Parameters.AddWithValue("$DeepCleanMinimum", (double)settings.DeepCleanMinimum);
                insert.Parameters.AddWithValue("$MaintenanceRate", (double)settings.MaintenanceRate);
                insert.Parameters.AddWithValue("$MaintenanceMinimum", (double)settings.MaintenanceMinimum);
                insert.Parameters.AddWithValue("$OneTimeDeepCleanRate", (double)settings.OneTimeDeepCleanRate);
                insert.Parameters.AddWithValue("$OneTimeDeepCleanMinimum", (double)settings.OneTimeDeepCleanMinimum);
                insert.Parameters.AddWithValue("$WindowInsideRate", (double)settings.WindowInsideRate);
                insert.Parameters.AddWithValue("$WindowOutsideRate", (double)settings.WindowOutsideRate);
                insert.Parameters.AddWithValue("$UpdatedAt", settings.UpdatedAt.ToString("o"));

                insert.ExecuteNonQuery();
            }
        }

        private static string GetOptionalString(System.Data.Common.DbDataReader reader, int index)
        {
            return reader.IsDBNull(index) ? "" : reader.GetString(index);
        }

        private static decimal GetOptionalDecimal(System.Data.Common.DbDataReader reader, int index, decimal fallback)
        {
            return reader.IsDBNull(index) ? fallback : Convert.ToDecimal(reader.GetDouble(index));
        }

        private static int GetOptionalInt(System.Data.Common.DbDataReader reader, int index, int fallback)
        {
            return reader.IsDBNull(index) ? fallback : reader.GetInt32(index);
        }
    }
}

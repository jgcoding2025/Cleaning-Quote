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
       SizeSmallMultiplier, SizeMediumMultiplier, SizeLargeMultiplier,
       SizeSmallDefinition, SizeMediumDefinition, SizeLargeDefinition,
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
       DefaultRoomType, DefaultRoomLevel, DefaultRoomSize, DefaultRoomComplexity,
       DefaultSubItemType, DefaultWindowSize,
       RoomBathroomFullMinutes, RoomBathroomHalfMinutes, RoomBathroomMasterMinutes,
       RoomBedroomMinutes, RoomBedroomMasterMinutes, RoomDiningRoomMinutes,
       RoomEntryMinutes, RoomFamilyRoomMinutes, RoomHallwayMinutes,
       RoomKitchenMinutes, RoomLaundryMinutes, RoomLivingRoomMinutes, RoomOfficeMinutes,
       SubItemCeilingFanMinutes, SubItemFridgeMinutes, SubItemMirrorMinutes, SubItemOvenMinutes,
       SubItemShowerNoGlassMinutes, SubItemShowerNoStoneMinutes, SubItemSinkDiscountMinutes,
       SubItemStoveTopGasMinutes, SubItemTubMinutes,
       SubItemWindowInsideFirstMinutes, SubItemWindowOutsideFirstMinutes,
       SubItemWindowInsideSecondMinutes, SubItemWindowOutsideSecondMinutes,
       SubItemWindowTrackMinutes, SubItemWindowStandardMinutes,
       UpdatedAt
FROM ServiceTypePricing
WHERE ServiceType = $ServiceType;
";
            cmd.Parameters.AddWithValue("$ServiceType", normalized);

            ServiceTypePricing pricing = null;
            var needsUpdate = false;

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var defaults = ServiceTypePricing.Default(normalized);
                    var sizeSmallMultiplier = GetOptionalDecimal(reader, 5, defaults.SizeSmallMultiplier);
                    var sizeMediumMultiplier = GetOptionalDecimal(reader, 6, defaults.SizeMediumMultiplier);
                    var sizeLargeMultiplier = GetOptionalDecimal(reader, 7, defaults.SizeLargeMultiplier);
                    var sizeSmallDefinition = GetOptionalString(reader, 8);
                    var sizeMediumDefinition = GetOptionalString(reader, 9);
                    var sizeLargeDefinition = GetOptionalString(reader, 10);
                    var windowInsideRate = GetOptionalDecimal(reader, 39, defaults.WindowInsideRate);
                    var windowOutsideRate = GetOptionalDecimal(reader, 40, defaults.WindowOutsideRate);
                    var defaultRoomType = GetOptionalString(reader, 41);
                    var defaultRoomLevel = GetOptionalString(reader, 42);
                    var defaultRoomSize = GetOptionalString(reader, 43);
                    var defaultRoomComplexity = GetOptionalInt(reader, 44, defaults.DefaultRoomComplexity);
                    var defaultSubItemType = GetOptionalString(reader, 45);
                    var defaultWindowSize = GetOptionalString(reader, 46);

                    if (windowInsideRate <= 0m)
                    {
                        windowInsideRate = defaults.WindowInsideRate;
                        needsUpdate = true;
                    }

                    if (sizeSmallMultiplier <= 0m)
                    {
                        sizeSmallMultiplier = defaults.SizeSmallMultiplier;
                        needsUpdate = true;
                    }

                    if (sizeMediumMultiplier <= 0m)
                    {
                        sizeMediumMultiplier = defaults.SizeMediumMultiplier;
                        needsUpdate = true;
                    }

                    if (sizeLargeMultiplier <= 0m)
                    {
                        sizeLargeMultiplier = defaults.SizeLargeMultiplier;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(sizeSmallDefinition))
                    {
                        sizeSmallDefinition = defaults.SizeSmallDefinition;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(sizeMediumDefinition))
                    {
                        sizeMediumDefinition = defaults.SizeMediumDefinition;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(sizeLargeDefinition))
                    {
                        sizeLargeDefinition = defaults.SizeLargeDefinition;
                        needsUpdate = true;
                    }

                    if (windowOutsideRate <= 0m)
                    {
                        windowOutsideRate = defaults.WindowOutsideRate;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(defaultRoomType))
                    {
                        defaultRoomType = defaults.DefaultRoomType;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(defaultRoomLevel))
                    {
                        defaultRoomLevel = defaults.DefaultRoomLevel;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(defaultRoomSize))
                    {
                        defaultRoomSize = defaults.DefaultRoomSize;
                        needsUpdate = true;
                    }

                    if (defaultRoomComplexity <= 0)
                    {
                        defaultRoomComplexity = defaults.DefaultRoomComplexity;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(defaultSubItemType))
                    {
                        defaultSubItemType = defaults.DefaultSubItemType;
                        needsUpdate = true;
                    }

                    if (string.IsNullOrWhiteSpace(defaultWindowSize))
                    {
                        defaultWindowSize = defaults.DefaultWindowSize;
                        needsUpdate = true;
                    }

                    pricing = new ServiceTypePricing
                    {
                        ServiceType = reader.GetString(0),
                        SqFtPerLaborHour = Convert.ToDecimal(reader.GetDouble(1)),
                        SizeSmallSqFt = Convert.ToDecimal(reader.GetDouble(2)),
                        SizeMediumSqFt = Convert.ToDecimal(reader.GetDouble(3)),
                        SizeLargeSqFt = Convert.ToDecimal(reader.GetDouble(4)),
                        SizeSmallMultiplier = sizeSmallMultiplier,
                        SizeMediumMultiplier = sizeMediumMultiplier,
                        SizeLargeMultiplier = sizeLargeMultiplier,
                        SizeSmallDefinition = sizeSmallDefinition,
                        SizeMediumDefinition = sizeMediumDefinition,
                        SizeLargeDefinition = sizeLargeDefinition,
                        Complexity1Multiplier = Convert.ToDecimal(reader.GetDouble(11)),
                        Complexity2Multiplier = Convert.ToDecimal(reader.GetDouble(12)),
                        Complexity3Multiplier = Convert.ToDecimal(reader.GetDouble(13)),
                        Complexity1Definition = GetOptionalString(reader, 14),
                        Complexity2Definition = GetOptionalString(reader, 15),
                        Complexity3Definition = GetOptionalString(reader, 16),
                        FullGlassShowerHoursEach = Convert.ToDecimal(reader.GetDouble(17)),
                        FullGlassShowerComplexity = GetOptionalInt(reader, 18, defaults.FullGlassShowerComplexity),
                        PebbleStoneFloorHoursEach = Convert.ToDecimal(reader.GetDouble(19)),
                        PebbleStoneFloorComplexity = GetOptionalInt(reader, 20, defaults.PebbleStoneFloorComplexity),
                        FridgeHoursEach = Convert.ToDecimal(reader.GetDouble(21)),
                        FridgeComplexity = GetOptionalInt(reader, 22, defaults.FridgeComplexity),
                        OvenHoursEach = Convert.ToDecimal(reader.GetDouble(23)),
                        OvenComplexity = GetOptionalInt(reader, 24, defaults.OvenComplexity),
                        CeilingFanHoursEach = GetOptionalDecimal(reader, 25, defaults.CeilingFanHoursEach),
                        CeilingFanComplexity = GetOptionalInt(reader, 26, defaults.CeilingFanComplexity),
                        WindowSmallHoursEach = GetOptionalDecimal(reader, 27, defaults.WindowSmallHoursEach),
                        WindowMediumHoursEach = GetOptionalDecimal(reader, 28, defaults.WindowMediumHoursEach),
                        WindowLargeHoursEach = GetOptionalDecimal(reader, 29, defaults.WindowLargeHoursEach),
                        WindowComplexity = GetOptionalInt(reader, 30, defaults.WindowComplexity),
                        FirstCleanRate = GetOptionalDecimal(reader, 31, defaults.FirstCleanRate),
                        FirstCleanMinimum = GetOptionalDecimal(reader, 32, defaults.FirstCleanMinimum),
                        DeepCleanRate = GetOptionalDecimal(reader, 33, defaults.DeepCleanRate),
                        DeepCleanMinimum = GetOptionalDecimal(reader, 34, defaults.DeepCleanMinimum),
                        MaintenanceRate = GetOptionalDecimal(reader, 35, defaults.MaintenanceRate),
                        MaintenanceMinimum = GetOptionalDecimal(reader, 36, defaults.MaintenanceMinimum),
                        OneTimeDeepCleanRate = GetOptionalDecimal(reader, 37, defaults.OneTimeDeepCleanRate),
                        OneTimeDeepCleanMinimum = GetOptionalDecimal(reader, 38, defaults.OneTimeDeepCleanMinimum),
                        WindowInsideRate = windowInsideRate,
                        WindowOutsideRate = windowOutsideRate,
                        DefaultRoomType = defaultRoomType,
                        DefaultRoomLevel = defaultRoomLevel,
                        DefaultRoomSize = defaultRoomSize,
                        DefaultRoomComplexity = defaultRoomComplexity,
                        DefaultSubItemType = defaultSubItemType,
                        DefaultWindowSize = defaultWindowSize,
                        RoomBathroomFullMinutes = GetOptionalInt(reader, 47, defaults.RoomBathroomFullMinutes),
                        RoomBathroomHalfMinutes = GetOptionalInt(reader, 48, defaults.RoomBathroomHalfMinutes),
                        RoomBathroomMasterMinutes = GetOptionalInt(reader, 49, defaults.RoomBathroomMasterMinutes),
                        RoomBedroomMinutes = GetOptionalInt(reader, 50, defaults.RoomBedroomMinutes),
                        RoomBedroomMasterMinutes = GetOptionalInt(reader, 51, defaults.RoomBedroomMasterMinutes),
                        RoomDiningRoomMinutes = GetOptionalInt(reader, 52, defaults.RoomDiningRoomMinutes),
                        RoomEntryMinutes = GetOptionalInt(reader, 53, defaults.RoomEntryMinutes),
                        RoomFamilyRoomMinutes = GetOptionalInt(reader, 54, defaults.RoomFamilyRoomMinutes),
                        RoomHallwayMinutes = GetOptionalInt(reader, 55, defaults.RoomHallwayMinutes),
                        RoomKitchenMinutes = GetOptionalInt(reader, 56, defaults.RoomKitchenMinutes),
                        RoomLaundryMinutes = GetOptionalInt(reader, 57, defaults.RoomLaundryMinutes),
                        RoomLivingRoomMinutes = GetOptionalInt(reader, 58, defaults.RoomLivingRoomMinutes),
                        RoomOfficeMinutes = GetOptionalInt(reader, 59, defaults.RoomOfficeMinutes),
                        SubItemCeilingFanMinutes = GetOptionalInt(reader, 60, defaults.SubItemCeilingFanMinutes),
                        SubItemFridgeMinutes = GetOptionalInt(reader, 61, defaults.SubItemFridgeMinutes),
                        SubItemMirrorMinutes = GetOptionalInt(reader, 62, defaults.SubItemMirrorMinutes),
                        SubItemOvenMinutes = GetOptionalInt(reader, 63, defaults.SubItemOvenMinutes),
                        SubItemShowerNoGlassMinutes = GetOptionalInt(reader, 64, defaults.SubItemShowerNoGlassMinutes),
                        SubItemShowerNoStoneMinutes = GetOptionalInt(reader, 65, defaults.SubItemShowerNoStoneMinutes),
                        SubItemSinkDiscountMinutes = GetOptionalInt(reader, 66, defaults.SubItemSinkDiscountMinutes),
                        SubItemStoveTopGasMinutes = GetOptionalInt(reader, 67, defaults.SubItemStoveTopGasMinutes),
                        SubItemTubMinutes = GetOptionalInt(reader, 68, defaults.SubItemTubMinutes),
                        SubItemWindowInsideFirstMinutes = GetOptionalInt(reader, 69, defaults.SubItemWindowInsideFirstMinutes),
                        SubItemWindowOutsideFirstMinutes = GetOptionalInt(reader, 70, defaults.SubItemWindowOutsideFirstMinutes),
                        SubItemWindowInsideSecondMinutes = GetOptionalInt(reader, 71, defaults.SubItemWindowInsideSecondMinutes),
                        SubItemWindowOutsideSecondMinutes = GetOptionalInt(reader, 72, defaults.SubItemWindowOutsideSecondMinutes),
                        SubItemWindowTrackMinutes = GetOptionalInt(reader, 73, defaults.SubItemWindowTrackMinutes),
                        SubItemWindowStandardMinutes = GetOptionalInt(reader, 74, defaults.SubItemWindowStandardMinutes),
                        UpdatedAt = DateTime.Parse(reader.GetString(75))
                    };
                }
            }

            if (pricing != null)
            {
                if (needsUpdate)
                    Upsert(pricing);

                return pricing;
            }

            var createdDefaults = ServiceTypePricing.Default(normalized);
            Upsert(createdDefaults);
            return createdDefaults;
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
    SizeSmallMultiplier = $SizeSmallMultiplier,
    SizeMediumMultiplier = $SizeMediumMultiplier,
    SizeLargeMultiplier = $SizeLargeMultiplier,
    SizeSmallDefinition = $SizeSmallDefinition,
    SizeMediumDefinition = $SizeMediumDefinition,
    SizeLargeDefinition = $SizeLargeDefinition,
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
    DefaultRoomType = $DefaultRoomType,
    DefaultRoomLevel = $DefaultRoomLevel,
    DefaultRoomSize = $DefaultRoomSize,
    DefaultRoomComplexity = $DefaultRoomComplexity,
    DefaultSubItemType = $DefaultSubItemType,
    DefaultWindowSize = $DefaultWindowSize,
    RoomBathroomFullMinutes = $RoomBathroomFullMinutes,
    RoomBathroomHalfMinutes = $RoomBathroomHalfMinutes,
    RoomBathroomMasterMinutes = $RoomBathroomMasterMinutes,
    RoomBedroomMinutes = $RoomBedroomMinutes,
    RoomBedroomMasterMinutes = $RoomBedroomMasterMinutes,
    RoomDiningRoomMinutes = $RoomDiningRoomMinutes,
    RoomEntryMinutes = $RoomEntryMinutes,
    RoomFamilyRoomMinutes = $RoomFamilyRoomMinutes,
    RoomHallwayMinutes = $RoomHallwayMinutes,
    RoomKitchenMinutes = $RoomKitchenMinutes,
    RoomLaundryMinutes = $RoomLaundryMinutes,
    RoomLivingRoomMinutes = $RoomLivingRoomMinutes,
    RoomOfficeMinutes = $RoomOfficeMinutes,
    SubItemCeilingFanMinutes = $SubItemCeilingFanMinutes,
    SubItemFridgeMinutes = $SubItemFridgeMinutes,
    SubItemMirrorMinutes = $SubItemMirrorMinutes,
    SubItemOvenMinutes = $SubItemOvenMinutes,
    SubItemShowerNoGlassMinutes = $SubItemShowerNoGlassMinutes,
    SubItemShowerNoStoneMinutes = $SubItemShowerNoStoneMinutes,
    SubItemSinkDiscountMinutes = $SubItemSinkDiscountMinutes,
    SubItemStoveTopGasMinutes = $SubItemStoveTopGasMinutes,
    SubItemTubMinutes = $SubItemTubMinutes,
    SubItemWindowInsideFirstMinutes = $SubItemWindowInsideFirstMinutes,
    SubItemWindowOutsideFirstMinutes = $SubItemWindowOutsideFirstMinutes,
    SubItemWindowInsideSecondMinutes = $SubItemWindowInsideSecondMinutes,
    SubItemWindowOutsideSecondMinutes = $SubItemWindowOutsideSecondMinutes,
    SubItemWindowTrackMinutes = $SubItemWindowTrackMinutes,
    SubItemWindowStandardMinutes = $SubItemWindowStandardMinutes,
    UpdatedAt = $UpdatedAt
WHERE ServiceType = $ServiceType;
";
                cmd.Parameters.AddWithValue("$ServiceType", normalized);
                cmd.Parameters.AddWithValue("$SqFtPerLaborHour", (double)settings.SqFtPerLaborHour);
                cmd.Parameters.AddWithValue("$SizeSmallSqFt", (double)settings.SizeSmallSqFt);
                cmd.Parameters.AddWithValue("$SizeMediumSqFt", (double)settings.SizeMediumSqFt);
                cmd.Parameters.AddWithValue("$SizeLargeSqFt", (double)settings.SizeLargeSqFt);
                cmd.Parameters.AddWithValue("$SizeSmallMultiplier", (double)settings.SizeSmallMultiplier);
                cmd.Parameters.AddWithValue("$SizeMediumMultiplier", (double)settings.SizeMediumMultiplier);
                cmd.Parameters.AddWithValue("$SizeLargeMultiplier", (double)settings.SizeLargeMultiplier);
                cmd.Parameters.AddWithValue("$SizeSmallDefinition", settings.SizeSmallDefinition ?? "");
                cmd.Parameters.AddWithValue("$SizeMediumDefinition", settings.SizeMediumDefinition ?? "");
                cmd.Parameters.AddWithValue("$SizeLargeDefinition", settings.SizeLargeDefinition ?? "");
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
                cmd.Parameters.AddWithValue("$DefaultRoomType", settings.DefaultRoomType ?? "");
                cmd.Parameters.AddWithValue("$DefaultRoomLevel", settings.DefaultRoomLevel ?? "");
                cmd.Parameters.AddWithValue("$DefaultRoomSize", settings.DefaultRoomSize ?? "");
                cmd.Parameters.AddWithValue("$DefaultRoomComplexity", settings.DefaultRoomComplexity);
                cmd.Parameters.AddWithValue("$DefaultSubItemType", settings.DefaultSubItemType ?? "");
                cmd.Parameters.AddWithValue("$DefaultWindowSize", settings.DefaultWindowSize ?? "");
                cmd.Parameters.AddWithValue("$RoomBathroomFullMinutes", settings.RoomBathroomFullMinutes);
                cmd.Parameters.AddWithValue("$RoomBathroomHalfMinutes", settings.RoomBathroomHalfMinutes);
                cmd.Parameters.AddWithValue("$RoomBathroomMasterMinutes", settings.RoomBathroomMasterMinutes);
                cmd.Parameters.AddWithValue("$RoomBedroomMinutes", settings.RoomBedroomMinutes);
                cmd.Parameters.AddWithValue("$RoomBedroomMasterMinutes", settings.RoomBedroomMasterMinutes);
                cmd.Parameters.AddWithValue("$RoomDiningRoomMinutes", settings.RoomDiningRoomMinutes);
                cmd.Parameters.AddWithValue("$RoomEntryMinutes", settings.RoomEntryMinutes);
                cmd.Parameters.AddWithValue("$RoomFamilyRoomMinutes", settings.RoomFamilyRoomMinutes);
                cmd.Parameters.AddWithValue("$RoomHallwayMinutes", settings.RoomHallwayMinutes);
                cmd.Parameters.AddWithValue("$RoomKitchenMinutes", settings.RoomKitchenMinutes);
                cmd.Parameters.AddWithValue("$RoomLaundryMinutes", settings.RoomLaundryMinutes);
                cmd.Parameters.AddWithValue("$RoomLivingRoomMinutes", settings.RoomLivingRoomMinutes);
                cmd.Parameters.AddWithValue("$RoomOfficeMinutes", settings.RoomOfficeMinutes);
                cmd.Parameters.AddWithValue("$SubItemCeilingFanMinutes", settings.SubItemCeilingFanMinutes);
                cmd.Parameters.AddWithValue("$SubItemFridgeMinutes", settings.SubItemFridgeMinutes);
                cmd.Parameters.AddWithValue("$SubItemMirrorMinutes", settings.SubItemMirrorMinutes);
                cmd.Parameters.AddWithValue("$SubItemOvenMinutes", settings.SubItemOvenMinutes);
                cmd.Parameters.AddWithValue("$SubItemShowerNoGlassMinutes", settings.SubItemShowerNoGlassMinutes);
                cmd.Parameters.AddWithValue("$SubItemShowerNoStoneMinutes", settings.SubItemShowerNoStoneMinutes);
                cmd.Parameters.AddWithValue("$SubItemSinkDiscountMinutes", settings.SubItemSinkDiscountMinutes);
                cmd.Parameters.AddWithValue("$SubItemStoveTopGasMinutes", settings.SubItemStoveTopGasMinutes);
                cmd.Parameters.AddWithValue("$SubItemTubMinutes", settings.SubItemTubMinutes);
                cmd.Parameters.AddWithValue("$SubItemWindowInsideFirstMinutes", settings.SubItemWindowInsideFirstMinutes);
                cmd.Parameters.AddWithValue("$SubItemWindowOutsideFirstMinutes", settings.SubItemWindowOutsideFirstMinutes);
                cmd.Parameters.AddWithValue("$SubItemWindowInsideSecondMinutes", settings.SubItemWindowInsideSecondMinutes);
                cmd.Parameters.AddWithValue("$SubItemWindowOutsideSecondMinutes", settings.SubItemWindowOutsideSecondMinutes);
                cmd.Parameters.AddWithValue("$SubItemWindowTrackMinutes", settings.SubItemWindowTrackMinutes);
                cmd.Parameters.AddWithValue("$SubItemWindowStandardMinutes", settings.SubItemWindowStandardMinutes);
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
 SizeSmallMultiplier, SizeMediumMultiplier, SizeLargeMultiplier,
 SizeSmallDefinition, SizeMediumDefinition, SizeLargeDefinition,
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
 DefaultRoomType, DefaultRoomLevel, DefaultRoomSize, DefaultRoomComplexity,
 DefaultSubItemType, DefaultWindowSize,
 RoomBathroomFullMinutes, RoomBathroomHalfMinutes, RoomBathroomMasterMinutes,
 RoomBedroomMinutes, RoomBedroomMasterMinutes, RoomDiningRoomMinutes,
 RoomEntryMinutes, RoomFamilyRoomMinutes, RoomHallwayMinutes,
 RoomKitchenMinutes, RoomLaundryMinutes, RoomLivingRoomMinutes, RoomOfficeMinutes,
 SubItemCeilingFanMinutes, SubItemFridgeMinutes, SubItemMirrorMinutes, SubItemOvenMinutes,
 SubItemShowerNoGlassMinutes, SubItemShowerNoStoneMinutes, SubItemSinkDiscountMinutes,
 SubItemStoveTopGasMinutes, SubItemTubMinutes,
 SubItemWindowInsideFirstMinutes, SubItemWindowOutsideFirstMinutes,
 SubItemWindowInsideSecondMinutes, SubItemWindowOutsideSecondMinutes,
 SubItemWindowTrackMinutes, SubItemWindowStandardMinutes,
 UpdatedAt)
VALUES
($ServiceType, $SqFtPerLaborHour, $SizeSmallSqFt, $SizeMediumSqFt, $SizeLargeSqFt,
 $SizeSmallMultiplier, $SizeMediumMultiplier, $SizeLargeMultiplier,
 $SizeSmallDefinition, $SizeMediumDefinition, $SizeLargeDefinition,
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
 $DefaultRoomType, $DefaultRoomLevel, $DefaultRoomSize, $DefaultRoomComplexity,
 $DefaultSubItemType, $DefaultWindowSize,
 $RoomBathroomFullMinutes, $RoomBathroomHalfMinutes, $RoomBathroomMasterMinutes,
 $RoomBedroomMinutes, $RoomBedroomMasterMinutes, $RoomDiningRoomMinutes,
 $RoomEntryMinutes, $RoomFamilyRoomMinutes, $RoomHallwayMinutes,
 $RoomKitchenMinutes, $RoomLaundryMinutes, $RoomLivingRoomMinutes, $RoomOfficeMinutes,
 $SubItemCeilingFanMinutes, $SubItemFridgeMinutes, $SubItemMirrorMinutes, $SubItemOvenMinutes,
 $SubItemShowerNoGlassMinutes, $SubItemShowerNoStoneMinutes, $SubItemSinkDiscountMinutes,
 $SubItemStoveTopGasMinutes, $SubItemTubMinutes,
 $SubItemWindowInsideFirstMinutes, $SubItemWindowOutsideFirstMinutes,
 $SubItemWindowInsideSecondMinutes, $SubItemWindowOutsideSecondMinutes,
 $SubItemWindowTrackMinutes, $SubItemWindowStandardMinutes,
 $UpdatedAt);
";
                insert.Parameters.AddWithValue("$ServiceType", normalized);
                insert.Parameters.AddWithValue("$SqFtPerLaborHour", (double)settings.SqFtPerLaborHour);
                insert.Parameters.AddWithValue("$SizeSmallSqFt", (double)settings.SizeSmallSqFt);
                insert.Parameters.AddWithValue("$SizeMediumSqFt", (double)settings.SizeMediumSqFt);
                insert.Parameters.AddWithValue("$SizeLargeSqFt", (double)settings.SizeLargeSqFt);
                insert.Parameters.AddWithValue("$SizeSmallMultiplier", (double)settings.SizeSmallMultiplier);
                insert.Parameters.AddWithValue("$SizeMediumMultiplier", (double)settings.SizeMediumMultiplier);
                insert.Parameters.AddWithValue("$SizeLargeMultiplier", (double)settings.SizeLargeMultiplier);
                insert.Parameters.AddWithValue("$SizeSmallDefinition", settings.SizeSmallDefinition ?? "");
                insert.Parameters.AddWithValue("$SizeMediumDefinition", settings.SizeMediumDefinition ?? "");
                insert.Parameters.AddWithValue("$SizeLargeDefinition", settings.SizeLargeDefinition ?? "");
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
                insert.Parameters.AddWithValue("$DefaultRoomType", settings.DefaultRoomType ?? "");
                insert.Parameters.AddWithValue("$DefaultRoomLevel", settings.DefaultRoomLevel ?? "");
                insert.Parameters.AddWithValue("$DefaultRoomSize", settings.DefaultRoomSize ?? "");
                insert.Parameters.AddWithValue("$DefaultRoomComplexity", settings.DefaultRoomComplexity);
                insert.Parameters.AddWithValue("$DefaultSubItemType", settings.DefaultSubItemType ?? "");
                insert.Parameters.AddWithValue("$DefaultWindowSize", settings.DefaultWindowSize ?? "");
                insert.Parameters.AddWithValue("$RoomBathroomFullMinutes", settings.RoomBathroomFullMinutes);
                insert.Parameters.AddWithValue("$RoomBathroomHalfMinutes", settings.RoomBathroomHalfMinutes);
                insert.Parameters.AddWithValue("$RoomBathroomMasterMinutes", settings.RoomBathroomMasterMinutes);
                insert.Parameters.AddWithValue("$RoomBedroomMinutes", settings.RoomBedroomMinutes);
                insert.Parameters.AddWithValue("$RoomBedroomMasterMinutes", settings.RoomBedroomMasterMinutes);
                insert.Parameters.AddWithValue("$RoomDiningRoomMinutes", settings.RoomDiningRoomMinutes);
                insert.Parameters.AddWithValue("$RoomEntryMinutes", settings.RoomEntryMinutes);
                insert.Parameters.AddWithValue("$RoomFamilyRoomMinutes", settings.RoomFamilyRoomMinutes);
                insert.Parameters.AddWithValue("$RoomHallwayMinutes", settings.RoomHallwayMinutes);
                insert.Parameters.AddWithValue("$RoomKitchenMinutes", settings.RoomKitchenMinutes);
                insert.Parameters.AddWithValue("$RoomLaundryMinutes", settings.RoomLaundryMinutes);
                insert.Parameters.AddWithValue("$RoomLivingRoomMinutes", settings.RoomLivingRoomMinutes);
                insert.Parameters.AddWithValue("$RoomOfficeMinutes", settings.RoomOfficeMinutes);
                insert.Parameters.AddWithValue("$SubItemCeilingFanMinutes", settings.SubItemCeilingFanMinutes);
                insert.Parameters.AddWithValue("$SubItemFridgeMinutes", settings.SubItemFridgeMinutes);
                insert.Parameters.AddWithValue("$SubItemMirrorMinutes", settings.SubItemMirrorMinutes);
                insert.Parameters.AddWithValue("$SubItemOvenMinutes", settings.SubItemOvenMinutes);
                insert.Parameters.AddWithValue("$SubItemShowerNoGlassMinutes", settings.SubItemShowerNoGlassMinutes);
                insert.Parameters.AddWithValue("$SubItemShowerNoStoneMinutes", settings.SubItemShowerNoStoneMinutes);
                insert.Parameters.AddWithValue("$SubItemSinkDiscountMinutes", settings.SubItemSinkDiscountMinutes);
                insert.Parameters.AddWithValue("$SubItemStoveTopGasMinutes", settings.SubItemStoveTopGasMinutes);
                insert.Parameters.AddWithValue("$SubItemTubMinutes", settings.SubItemTubMinutes);
                insert.Parameters.AddWithValue("$SubItemWindowInsideFirstMinutes", settings.SubItemWindowInsideFirstMinutes);
                insert.Parameters.AddWithValue("$SubItemWindowOutsideFirstMinutes", settings.SubItemWindowOutsideFirstMinutes);
                insert.Parameters.AddWithValue("$SubItemWindowInsideSecondMinutes", settings.SubItemWindowInsideSecondMinutes);
                insert.Parameters.AddWithValue("$SubItemWindowOutsideSecondMinutes", settings.SubItemWindowOutsideSecondMinutes);
                insert.Parameters.AddWithValue("$SubItemWindowTrackMinutes", settings.SubItemWindowTrackMinutes);
                insert.Parameters.AddWithValue("$SubItemWindowStandardMinutes", settings.SubItemWindowStandardMinutes);
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

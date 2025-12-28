using System;

namespace Cleaning_Quote.Models
{
    public class ServiceTypePricing
    {
        public string ServiceType { get; set; } = "";
        public decimal SqFtPerLaborHour { get; set; } = 500m;
        public decimal SizeSmallSqFt { get; set; } = 250m;
        public decimal SizeMediumSqFt { get; set; } = 375m;
        public decimal SizeLargeSqFt { get; set; } = 500m;
        public decimal SizeSmallMultiplier { get; set; } = 0.9m;
        public decimal SizeMediumMultiplier { get; set; } = 1.0m;
        public decimal SizeLargeMultiplier { get; set; } = 1.1m;
        public string SizeSmallDefinition { get; set; } = "Small";
        public string SizeMediumDefinition { get; set; } = "Medium (average)";
        public string SizeLargeDefinition { get; set; } = "Large";
        public decimal Complexity1Multiplier { get; set; } = 0.75m;
        public decimal Complexity2Multiplier { get; set; } = 1.00m;
        public decimal Complexity3Multiplier { get; set; } = 1.25m;
        public string Complexity1Definition { get; set; } = "Light use with minimal buildup.";
        public string Complexity2Definition { get; set; } = "Moderate use with visible buildup (Average).";
        public string Complexity3Definition { get; set; } = "Heavy use with significant buildup.";
        public decimal FirstCleanRate { get; set; } = 0.15m;
        public decimal FirstCleanMinimum { get; set; } = 195m;
        public decimal DeepCleanRate { get; set; } = 0.20m;
        public decimal DeepCleanMinimum { get; set; } = 295m;
        public decimal MaintenanceRate { get; set; } = 0.10m;
        public decimal MaintenanceMinimum { get; set; } = 125m;
        public decimal OneTimeDeepCleanRate { get; set; } = 0.30m;
        public decimal OneTimeDeepCleanMinimum { get; set; } = 400m;
        public decimal WindowInsideRate { get; set; } = 4m;
        public decimal WindowOutsideRate { get; set; } = 4m;
        public string DefaultRoomType { get; set; } = "Bedroom";
        public string DefaultRoomLevel { get; set; } = "Main Floor (1)";
        public string DefaultRoomSize { get; set; } = "M";
        public int DefaultRoomComplexity { get; set; } = 2;
        public string DefaultSubItemType { get; set; } = "Ceiling Fan";
        public string DefaultWindowSize { get; set; } = "M";
        public int RoomBathroomFullMinutes { get; set; } = 60;
        public int RoomBathroomHalfMinutes { get; set; } = 20;
        public int RoomBathroomMasterMinutes { get; set; } = 120;
        public int RoomBedroomMinutes { get; set; } = 15;
        public int RoomBedroomMasterMinutes { get; set; } = 20;
        public int RoomDiningRoomMinutes { get; set; } = 20;
        public int RoomEntryMinutes { get; set; } = 8;
        public int RoomFamilyRoomMinutes { get; set; } = 20;
        public int RoomHallwayMinutes { get; set; } = 6;
        public int RoomKitchenMinutes { get; set; } = 60;
        public int RoomLaundryMinutes { get; set; } = 10;
        public int RoomLivingRoomMinutes { get; set; } = 20;
        public int RoomOfficeMinutes { get; set; } = 15;
        public int RoomBathroomFullSqFt { get; set; } = 50;
        public int RoomBathroomHalfSqFt { get; set; } = 25;
        public int RoomBathroomMasterSqFt { get; set; } = 100;
        public int RoomBedroomSqFt { get; set; } = 110;
        public int RoomBedroomMasterSqFt { get; set; } = 225;
        public int RoomDiningRoomSqFt { get; set; } = 140;
        public int RoomEntrySqFt { get; set; } = 45;
        public int RoomFamilyRoomSqFt { get; set; } = 300;
        public int RoomHallwaySqFt { get; set; } = 40;
        public int RoomKitchenSqFt { get; set; } = 175;
        public int RoomLaundrySqFt { get; set; } = 55;
        public int RoomLivingRoomSqFt { get; set; } = 300;
        public int RoomOfficeSqFt { get; set; } = 120;
        public int SubItemCeilingFanMinutes { get; set; } = 10;
        public int SubItemFridgeMinutes { get; set; } = 60;
        public int SubItemMirrorMinutes { get; set; } = 5;
        public int SubItemOvenMinutes { get; set; } = 60;
        public int SubItemShowerNoGlassMinutes { get; set; } = -20;
        public int SubItemShowerNoStoneMinutes { get; set; } = -20;
        public int SubItemSinkDiscountMinutes { get; set; } = -10;
        public int SubItemStoveTopGasMinutes { get; set; } = 30;
        public int SubItemTubMinutes { get; set; } = 25;
        public int SubItemWindowInsideFirstMinutes { get; set; } = 5;
        public int SubItemWindowOutsideFirstMinutes { get; set; } = 5;
        public int SubItemWindowInsideSecondMinutes { get; set; } = 10;
        public int SubItemWindowOutsideSecondMinutes { get; set; } = 10;
        public int SubItemWindowTrackMinutes { get; set; } = 5;
        public int SubItemWindowStandardMinutes { get; set; } = 10;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public static ServiceTypePricing Default(string serviceType)
        {
            var defaults = new ServiceTypePricing
            {
                ServiceType = serviceType ?? "",
                SqFtPerLaborHour = 500m,
                SizeSmallSqFt = 250m,
                SizeMediumSqFt = 375m,
                SizeLargeSqFt = 500m,
                SizeSmallMultiplier = 0.9m,
                SizeMediumMultiplier = 1.0m,
                SizeLargeMultiplier = 1.1m,
                SizeSmallDefinition = "Small",
                SizeMediumDefinition = "Medium (average)",
                SizeLargeDefinition = "Large",
                Complexity1Multiplier = 0.75m,
                Complexity2Multiplier = 1.00m,
                Complexity3Multiplier = 1.25m,
                Complexity1Definition = "Light use with minimal buildup.",
                Complexity2Definition = "Moderate use with visible buildup (Average).",
                Complexity3Definition = "Heavy use with significant buildup.",
                FirstCleanRate = 0.15m,
                FirstCleanMinimum = 195m,
                DeepCleanRate = 0.20m,
                DeepCleanMinimum = 295m,
                MaintenanceRate = 0.10m,
                MaintenanceMinimum = 125m,
                OneTimeDeepCleanRate = 0.30m,
                OneTimeDeepCleanMinimum = 400m,
                WindowInsideRate = 4m,
                WindowOutsideRate = 4m,
                DefaultRoomType = "Bedroom",
                DefaultRoomLevel = "Main Floor (1)",
                DefaultRoomSize = "M",
                DefaultRoomComplexity = 2,
                DefaultSubItemType = "Ceiling Fan",
                DefaultWindowSize = "M",
                RoomBathroomFullMinutes = 60,
                RoomBathroomHalfMinutes = 20,
                RoomBathroomMasterMinutes = 120,
                RoomBedroomMinutes = 15,
                RoomBedroomMasterMinutes = 20,
                RoomDiningRoomMinutes = 20,
                RoomEntryMinutes = 8,
                RoomFamilyRoomMinutes = 20,
                RoomHallwayMinutes = 6,
                RoomKitchenMinutes = 60,
                RoomLaundryMinutes = 10,
                RoomLivingRoomMinutes = 20,
                RoomOfficeMinutes = 15,
                RoomBathroomFullSqFt = 50,
                RoomBathroomHalfSqFt = 25,
                RoomBathroomMasterSqFt = 100,
                RoomBedroomSqFt = 110,
                RoomBedroomMasterSqFt = 225,
                RoomDiningRoomSqFt = 140,
                RoomEntrySqFt = 45,
                RoomFamilyRoomSqFt = 300,
                RoomHallwaySqFt = 40,
                RoomKitchenSqFt = 175,
                RoomLaundrySqFt = 55,
                RoomLivingRoomSqFt = 300,
                RoomOfficeSqFt = 120,
                SubItemCeilingFanMinutes = 10,
                SubItemFridgeMinutes = 60,
                SubItemMirrorMinutes = 5,
                SubItemOvenMinutes = 60,
                SubItemShowerNoGlassMinutes = -20,
                SubItemShowerNoStoneMinutes = -20,
                SubItemSinkDiscountMinutes = -10,
                SubItemStoveTopGasMinutes = 30,
                SubItemTubMinutes = 25,
                SubItemWindowInsideFirstMinutes = 5,
                SubItemWindowOutsideFirstMinutes = 5,
                SubItemWindowInsideSecondMinutes = 10,
                SubItemWindowOutsideSecondMinutes = 10,
                SubItemWindowTrackMinutes = 5,
                SubItemWindowStandardMinutes = 10,
                UpdatedAt = DateTime.UtcNow
            };

            if (TryGetServiceTypeRate(serviceType, out var ratePerSqFt))
            {
                defaults.FirstCleanRate = ratePerSqFt;
                defaults.DeepCleanRate = ratePerSqFt;
                defaults.MaintenanceRate = ratePerSqFt;
                defaults.OneTimeDeepCleanRate = ratePerSqFt;
            }

            return defaults;
        }

        private static bool TryGetServiceTypeRate(string serviceType, out decimal ratePerSqFt)
        {
            ratePerSqFt = 0m;
            if (string.IsNullOrWhiteSpace(serviceType))
                return false;

            var normalized = serviceType.Trim();
            switch (normalized)
            {
                case "Standard Clean":
                    ratePerSqFt = 0.12m;
                    return true;
                case "Initial Clean (Standard Clean Items Only)":
                case "Initial Clean":
                    ratePerSqFt = 0.16m;
                    return true;
                case "Deep Clean (one-time)":
                    ratePerSqFt = 0.30m;
                    return true;
                case "Deep Clean (post initial clean)":
                case "Deep Clean":
                    ratePerSqFt = 0.20m;
                    return true;
                case "Deep Clean (Semi-Annual)":
                    ratePerSqFt = 0.22m;
                    return true;
                case "Deep Clean (Annual)":
                    ratePerSqFt = 0.22m;
                    return true;
                case "Light Clean (dusting and floors)":
                case "Light Clean":
                    ratePerSqFt = 0.06m;
                    return true;
                case "Move In/Out Clean":
                case "Move in/out Clean":
                    ratePerSqFt = 0.32m;
                    return true;
                default:
                    return false;
            }
        }
    }
}

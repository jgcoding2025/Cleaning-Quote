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
        public decimal Complexity1Multiplier { get; set; } = 1.00m;
        public decimal Complexity2Multiplier { get; set; } = 1.25m;
        public decimal Complexity3Multiplier { get; set; } = 1.50m;
        public string Complexity1Definition { get; set; } = "Light use with minimal buildup.";
        public string Complexity2Definition { get; set; } = "Moderate use with visible buildup.";
        public string Complexity3Definition { get; set; } = "Heavy use with significant buildup or clutter.";
        public decimal FullGlassShowerHoursEach { get; set; } = 0.30m;
        public int FullGlassShowerComplexity { get; set; } = 2;
        public decimal PebbleStoneFloorHoursEach { get; set; } = 0.25m;
        public int PebbleStoneFloorComplexity { get; set; } = 2;
        public decimal FridgeHoursEach { get; set; } = 0.30m;
        public int FridgeComplexity { get; set; } = 2;
        public decimal OvenHoursEach { get; set; } = 0.35m;
        public int OvenComplexity { get; set; } = 2;
        public decimal CeilingFanHoursEach { get; set; } = 0.15m;
        public int CeilingFanComplexity { get; set; } = 1;
        public decimal WindowSmallHoursEach { get; set; } = 0.08m;
        public decimal WindowMediumHoursEach { get; set; } = 0.12m;
        public decimal WindowLargeHoursEach { get; set; } = 0.18m;
        public int WindowComplexity { get; set; } = 1;
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
        public int DefaultRoomComplexity { get; set; } = 1;
        public string DefaultSubItemType { get; set; } = "Full Glass Shower";
        public string DefaultWindowSize { get; set; } = "M";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public static ServiceTypePricing Default(string serviceType)
        {
            return new ServiceTypePricing
            {
                ServiceType = serviceType ?? "",
                SqFtPerLaborHour = 500m,
                SizeSmallSqFt = 250m,
                SizeMediumSqFt = 375m,
                SizeLargeSqFt = 500m,
                Complexity1Multiplier = 1.00m,
                Complexity2Multiplier = 1.25m,
                Complexity3Multiplier = 1.50m,
                Complexity1Definition = "Light use with minimal buildup.",
                Complexity2Definition = "Moderate use with visible buildup.",
                Complexity3Definition = "Heavy use with significant buildup or clutter.",
                FullGlassShowerHoursEach = 0.30m,
                FullGlassShowerComplexity = 2,
                PebbleStoneFloorHoursEach = 0.25m,
                PebbleStoneFloorComplexity = 2,
                FridgeHoursEach = 0.30m,
                FridgeComplexity = 2,
                OvenHoursEach = 0.35m,
                OvenComplexity = 2,
                CeilingFanHoursEach = 0.15m,
                CeilingFanComplexity = 1,
                WindowSmallHoursEach = 0.08m,
                WindowMediumHoursEach = 0.12m,
                WindowLargeHoursEach = 0.18m,
                WindowComplexity = 1,
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
                DefaultRoomComplexity = 1,
                DefaultSubItemType = "Full Glass Shower",
                DefaultWindowSize = "M",
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}

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
        public decimal FullGlassShowerHoursEach { get; set; } = 0.30m;
        public decimal PebbleStoneFloorHoursEach { get; set; } = 0.25m;
        public decimal FridgeHoursEach { get; set; } = 0.30m;
        public decimal OvenHoursEach { get; set; } = 0.35m;
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
                FullGlassShowerHoursEach = 0.30m,
                PebbleStoneFloorHoursEach = 0.25m,
                FridgeHoursEach = 0.30m,
                OvenHoursEach = 0.35m,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}

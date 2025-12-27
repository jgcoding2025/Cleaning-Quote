using System;
using System.Collections.Generic;
using Cleaning_Quote.Models;

namespace Cleaning_Quote.Services
{
    public sealed class QuoteTotals
    {
        public decimal TotalLaborHours { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CreditCardFee { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }

    public sealed class PricingRules
    {
        // Base hours: RoomType -> Size(S/M/L) -> hours
        public Dictionary<string, Dictionary<string, decimal>> BaseHours { get; } =
            new Dictionary<string, Dictionary<string, decimal>>(StringComparer.OrdinalIgnoreCase);

        // Complexity: 1/2/3 -> multiplier
        public Dictionary<int, decimal> ComplexityMultiplier { get; } =
            new Dictionary<int, decimal>();

        // Add-on hours (per item)
        public decimal FullGlassShowerHoursEach { get; set; }
        public decimal PebbleStoneFloorHoursEach { get; set; }
        public decimal FridgeHoursEach { get; set; }
        public decimal OvenHoursEach { get; set; }

        // House modifiers (as added hours)
        public decimal HoursPerPet { get; set; }
        public decimal SmokingInsideHours { get; set; }
        public decimal HouseholdSize_1to2_Hours { get; set; }
        public decimal HouseholdSize_3to4_Hours { get; set; }
        public decimal HouseholdSize_5plus_Hours { get; set; }

        // Rounding increment for labor hours (0.25 = quarter-hour)
        public decimal HourRoundingIncrement { get; set; } = 0.25m;

        public static PricingRules Default()
        {
            var r = new PricingRules();

            // Typical starting multipliers (edit as you like)
            r.ComplexityMultiplier[1] = 1.00m;
            r.ComplexityMultiplier[2] = 1.25m;
            r.ComplexityMultiplier[3] = 1.50m;

            // Typical starting base hours (EDIT THESE TO MATCH YOUR BUSINESS)
            r.BaseHours["Bedroom"] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["S"] = 0.50m, ["M"] = 0.75m, ["L"] = 1.00m };
            r.BaseHours["Bathroom"] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["S"] = 0.75m, ["M"] = 1.00m, ["L"] = 1.25m };
            r.BaseHours["Kitchen"] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["S"] = 1.00m, ["M"] = 1.25m, ["L"] = 1.50m };
            r.BaseHours["LivingRoom"] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["S"] = 0.75m, ["M"] = 1.00m, ["L"] = 1.25m };
            r.BaseHours["Playroom"] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["S"] = 0.75m, ["M"] = 1.00m, ["L"] = 1.25m };
            r.BaseHours["Hallway"] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["S"] = 0.25m, ["M"] = 0.35m, ["L"] = 0.50m };
            r.BaseHours["Entryway"] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["S"] = 0.25m, ["M"] = 0.35m, ["L"] = 0.50m };
            r.BaseHours["Staircase"] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["S"] = 0.35m, ["M"] = 0.50m, ["L"] = 0.75m };

            // Add-ons (edit)
            r.FullGlassShowerHoursEach = 0.30m;
            r.PebbleStoneFloorHoursEach = 0.25m;
            r.FridgeHoursEach = 0.30m;
            r.OvenHoursEach = 0.35m;

            // House modifiers (edit)
            r.HoursPerPet = 0.10m;            // +0.10 hours per pet
            r.SmokingInsideHours = 0.50m;      // +0.5 hours if smoking inside

            r.HouseholdSize_1to2_Hours = 0.00m;
            r.HouseholdSize_3to4_Hours = 0.25m;
            r.HouseholdSize_5plus_Hours = 0.50m;

            // Rounding (edit or set to 0 for none)
            r.HourRoundingIncrement = 0.25m;

            return r;
        }
    }

    public sealed class PricingService
    {
        private readonly PricingRules _rules;

        public PricingService(PricingRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public QuoteTotals CalculateTotals(Quote quote)
        {
            if (quote == null) throw new ArgumentNullException(nameof(quote));
            if (quote.LaborRate < 0) throw new ArgumentOutOfRangeException(nameof(quote.LaborRate));
            if (quote.TaxRate < 0 || quote.TaxRate > 1) throw new ArgumentOutOfRangeException(nameof(quote.TaxRate));
            if (quote.CreditCardFeeRate < 0 || quote.CreditCardFeeRate > 1) throw new ArgumentOutOfRangeException(nameof(quote.CreditCardFeeRate));

            decimal roomsHours = 0m;

            foreach (var room in quote.Rooms)
            {
                var h = CalculateRoomHours(room);

                // Snapshot at quote time (helpful for history)
                room.RoomLaborHours = h;

                roomsHours += h;
            }

            decimal houseHours = CalculateHouseModifierHours(quote);
            decimal totalHours = roomsHours + houseHours;
            totalHours = RoundHours(totalHours, _rules.HourRoundingIncrement);

            decimal subtotal = totalHours * quote.LaborRate;

            decimal ccFee = 0m;
            if (quote.CreditCard)
                ccFee = subtotal * quote.CreditCardFeeRate;

            decimal taxable = subtotal + ccFee;
            decimal tax = taxable * quote.TaxRate;

            var total = taxable + tax;

            return new QuoteTotals
            {
                TotalLaborHours = totalHours,
                Subtotal = subtotal,
                CreditCardFee = ccFee,
                Tax = tax,
                Total = total
            };
        }

        public decimal CalculateRoomHours(QuoteRoom room)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            string roomType = room.RoomType ?? "";
            string size = NormalizeSize(room.Size);
            int complexity = room.Complexity;

            decimal baseHours = GetBaseHours(roomType, size);
            decimal mult = GetComplexityMultiplier(complexity);

            decimal addon = 0m;
            addon += SafeCount(room.FullGlassShowersCount) * _rules.FullGlassShowerHoursEach;
            addon += SafeCount(room.PebbleStoneFloorsCount) * _rules.PebbleStoneFloorHoursEach;
            addon += SafeCount(room.FridgeCount) * _rules.FridgeHoursEach;
            addon += SafeCount(room.OvenCount) * _rules.OvenHoursEach;

            decimal roomHours = (baseHours * mult) + addon;

            // Round each room to keep behavior consistent (optional).
            // Comment out if you want only final rounding.
            roomHours = RoundHours(roomHours, _rules.HourRoundingIncrement);

            return roomHours;
        }

        private decimal CalculateHouseModifierHours(Quote quote)
        {
            decimal extra = 0m;

            if (quote.PetsCount > 0)
                extra += quote.PetsCount * _rules.HoursPerPet;

            if (quote.SmokingInside)
                extra += _rules.SmokingInsideHours;

            if (quote.HouseholdSize <= 2)
                extra += _rules.HouseholdSize_1to2_Hours;
            else if (quote.HouseholdSize <= 4)
                extra += _rules.HouseholdSize_3to4_Hours;
            else
                extra += _rules.HouseholdSize_5plus_Hours;

            return extra;
        }

        private decimal GetBaseHours(string roomType, string size)
        {
            if (!_rules.BaseHours.TryGetValue(roomType, out var bySize))
                throw new InvalidOperationException($"No base hours configured for room type '{roomType}'.");

            if (!bySize.TryGetValue(size, out var hours))
                throw new InvalidOperationException($"No base hours configured for room type '{roomType}' size '{size}'.");

            return hours;
        }

        private decimal GetComplexityMultiplier(int complexity)
        {
            if (!_rules.ComplexityMultiplier.TryGetValue(complexity, out var mult))
                throw new InvalidOperationException($"No complexity multiplier configured for complexity '{complexity}'.");

            return mult;
        }

        private static int SafeCount(int? value) => value.HasValue && value.Value > 0 ? value.Value : 0;

        private static string NormalizeSize(string size)
        {
            var s = (size ?? "").Trim().ToUpperInvariant();
            if (s == "SM" || s == "SMALL") return "S";
            if (s == "MD" || s == "MED" || s == "MEDIUM") return "M";
            if (s == "LG" || s == "LARGE") return "L";
            if (s == "S" || s == "M" || s == "L") return s;

            // Default to Medium if not specified
            return "M";
        }

        private static decimal RoundHours(decimal hours, decimal increment)
        {
            if (increment <= 0) return hours;

            // round to nearest increment (ex: 0.25)
            var factor = 1m / increment;
            return Math.Round(hours * factor, MidpointRounding.AwayFromZero) / factor;
        }
    }
}

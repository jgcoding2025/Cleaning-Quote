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
        // Base hours: RoomType -> Size(S/M/L) -> hours (optional fallback)
        public Dictionary<string, Dictionary<string, decimal>> BaseHours { get; } =
            new Dictionary<string, Dictionary<string, decimal>>(StringComparer.OrdinalIgnoreCase);

        // Size square footage: Size(S/M/L) -> sqft
        public Dictionary<string, decimal> SizeSquareFootage { get; } =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        // Labor rate: sqft cleaned per labor hour
        public decimal SqFtPerLaborHour { get; set; }

        // Complexity: 1/2/3 -> multiplier
        public Dictionary<int, decimal> ComplexityMultiplier { get; } =
            new Dictionary<int, decimal>();

        // Add-on hours (per item)
        public decimal FullGlassShowerHoursEach { get; set; }
        public int FullGlassShowerComplexity { get; set; } = 2;
        public decimal PebbleStoneFloorHoursEach { get; set; }
        public int PebbleStoneFloorComplexity { get; set; } = 2;
        public decimal FridgeHoursEach { get; set; }
        public int FridgeComplexity { get; set; } = 2;
        public decimal OvenHoursEach { get; set; }
        public int OvenComplexity { get; set; } = 2;
        public decimal CeilingFanHoursEach { get; set; }
        public int CeilingFanComplexity { get; set; } = 1;
        public decimal WindowSmallHoursEach { get; set; }
        public decimal WindowMediumHoursEach { get; set; }
        public decimal WindowLargeHoursEach { get; set; }
        public int WindowComplexity { get; set; } = 1;

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

            // Typical starting square-foot defaults (edit as needed)
            r.SqFtPerLaborHour = 500m;
            r.SizeSquareFootage["S"] = 250m;
            r.SizeSquareFootage["M"] = 375m;
            r.SizeSquareFootage["L"] = 500m;

            // Add-ons (edit)
            r.FullGlassShowerHoursEach = 0.30m;
            r.FullGlassShowerComplexity = 2;
            r.PebbleStoneFloorHoursEach = 0.25m;
            r.PebbleStoneFloorComplexity = 2;
            r.FridgeHoursEach = 0.30m;
            r.FridgeComplexity = 2;
            r.OvenHoursEach = 0.35m;
            r.OvenComplexity = 2;
            r.CeilingFanHoursEach = 0.15m;
            r.CeilingFanComplexity = 1;
            r.WindowSmallHoursEach = 0.08m;
            r.WindowMediumHoursEach = 0.12m;
            r.WindowLargeHoursEach = 0.18m;
            r.WindowComplexity = 1;

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

        public static PricingRules FromServiceTypePricing(ServiceTypePricing settings)
        {
            var rules = Default();

            if (settings == null)
                return rules;

            rules.SqFtPerLaborHour = settings.SqFtPerLaborHour;
            rules.SizeSquareFootage["S"] = settings.SizeSmallSqFt;
            rules.SizeSquareFootage["M"] = settings.SizeMediumSqFt;
            rules.SizeSquareFootage["L"] = settings.SizeLargeSqFt;

            void AddRoomBaseHours(string roomType, int minutes)
            {
                if (minutes <= 0)
                    return;

                var baseHours = minutes / 60m;
                var sizeSmall = settings.SizeSmallMultiplier <= 0m ? 1m : settings.SizeSmallMultiplier;
                var sizeMedium = settings.SizeMediumMultiplier <= 0m ? 1m : settings.SizeMediumMultiplier;
                var sizeLarge = settings.SizeLargeMultiplier <= 0m ? 1m : settings.SizeLargeMultiplier;

                rules.BaseHours[roomType] = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    ["S"] = baseHours * sizeSmall,
                    ["M"] = baseHours * sizeMedium,
                    ["L"] = baseHours * sizeLarge
                };
            }

            AddRoomBaseHours("Bathroom (Full)", settings.RoomBathroomFullMinutes);
            AddRoomBaseHours("Bathroom (Half)", settings.RoomBathroomHalfMinutes);
            AddRoomBaseHours("Bathroom (Master - 2 sinks, glass/stone shower)", settings.RoomBathroomMasterMinutes);
            AddRoomBaseHours("Bedroom", settings.RoomBedroomMinutes);
            AddRoomBaseHours("Bedroom (Master)", settings.RoomBedroomMasterMinutes);
            AddRoomBaseHours("Dining Room", settings.RoomDiningRoomMinutes);
            AddRoomBaseHours("Entry", settings.RoomEntryMinutes);
            AddRoomBaseHours("Family Room", settings.RoomFamilyRoomMinutes);
            AddRoomBaseHours("Hallway", settings.RoomHallwayMinutes);
            AddRoomBaseHours("Kitchen", settings.RoomKitchenMinutes);
            AddRoomBaseHours("Laundry", settings.RoomLaundryMinutes);
            AddRoomBaseHours("Living Room", settings.RoomLivingRoomMinutes);
            AddRoomBaseHours("Office", settings.RoomOfficeMinutes);

            rules.ComplexityMultiplier[1] = settings.Complexity1Multiplier;
            rules.ComplexityMultiplier[2] = settings.Complexity2Multiplier;
            rules.ComplexityMultiplier[3] = settings.Complexity3Multiplier;

            rules.FullGlassShowerHoursEach = settings.FullGlassShowerHoursEach;
            rules.FullGlassShowerComplexity = settings.FullGlassShowerComplexity;
            rules.PebbleStoneFloorHoursEach = settings.PebbleStoneFloorHoursEach;
            rules.PebbleStoneFloorComplexity = settings.PebbleStoneFloorComplexity;
            rules.FridgeHoursEach = settings.FridgeHoursEach;
            rules.FridgeComplexity = settings.FridgeComplexity;
            rules.OvenHoursEach = settings.OvenHoursEach;
            rules.OvenComplexity = settings.OvenComplexity;
            rules.CeilingFanHoursEach = settings.CeilingFanHoursEach;
            rules.CeilingFanComplexity = settings.CeilingFanComplexity;
            rules.WindowSmallHoursEach = settings.WindowSmallHoursEach;
            rules.WindowMediumHoursEach = settings.WindowMediumHoursEach;
            rules.WindowLargeHoursEach = settings.WindowLargeHoursEach;
            rules.WindowComplexity = settings.WindowComplexity;

            return rules;
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
            if (!room.IncludedInQuote)
                return 0m;

            if (room.IsSubItem)
            {
                return CalculateSubItemHours(room);
            }

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

        private decimal CalculateSubItemHours(QuoteRoom item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!item.IncludedInQuote) return 0m;

            var category = item.ItemCategory ?? "";
            var baseHours = category switch
            {
                "FullGlassShower" => _rules.FullGlassShowerHoursEach,
                "PebbleStoneFloor" => _rules.PebbleStoneFloorHoursEach,
                "Fridge" => _rules.FridgeHoursEach,
                "Oven" => _rules.OvenHoursEach,
                "CeilingFan" => _rules.CeilingFanHoursEach,
                "Window" => GetWindowHours(item),
                _ => 0m
            };

            var complexity = category switch
            {
                "FullGlassShower" => _rules.FullGlassShowerComplexity,
                "PebbleStoneFloor" => _rules.PebbleStoneFloorComplexity,
                "Fridge" => _rules.FridgeComplexity,
                "Oven" => _rules.OvenComplexity,
                "CeilingFan" => _rules.CeilingFanComplexity,
                "Window" => _rules.WindowComplexity,
                _ => item.Complexity
            };

            var mult = GetComplexityMultiplier(complexity);
            var hours = baseHours * mult;
            return RoundHours(hours, _rules.HourRoundingIncrement);
        }

        private decimal GetWindowHours(QuoteRoom item)
        {
            var size = NormalizeSize(item.Size);
            var baseHours = size switch
            {
                "S" => _rules.WindowSmallHoursEach,
                "L" => _rules.WindowLargeHoursEach,
                _ => _rules.WindowMediumHoursEach
            };
            var sides = (item.WindowInside ? 1 : 0) + (item.WindowOutside ? 1 : 0);
            if (sides == 0)
                sides = 1;
            return baseHours * sides;
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
            if (_rules.BaseHours.TryGetValue(roomType, out var bySize) &&
                bySize.TryGetValue(size, out var hours))
            {
                return hours;
            }

            if (_rules.SqFtPerLaborHour > 0m &&
                _rules.SizeSquareFootage.TryGetValue(size, out var sqft) &&
                sqft > 0m)
            {
                return sqft / _rules.SqFtPerLaborHour;
            }

            throw new InvalidOperationException($"No base hours configured for room type '{roomType}' size '{size}'.");
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

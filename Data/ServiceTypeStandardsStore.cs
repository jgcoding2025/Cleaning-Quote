using Cleaning_Quote.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Cleaning_Quote.Data
{
    public static class ServiceTypeStandardsStore
    {
        private const string StandardsFileName = "ServiceTypeStandards.json";

        public static List<ServiceTypeStandard> Load(ServiceCatalog catalog)
        {
            var standards = ReadStandardsFile();
            if (catalog == null)
                return standards;

            var normalized = new Dictionary<string, ServiceTypeStandard>(StringComparer.OrdinalIgnoreCase);
            foreach (var standard in standards)
            {
                if (string.IsNullOrWhiteSpace(standard.ServiceType))
                    continue;

                standard.Multiplier = standard.Multiplier <= 0m ? 1m : standard.Multiplier;
                normalized[standard.ServiceType] = standard;
            }

            var merged = new List<ServiceTypeStandard>();
            foreach (var serviceType in catalog.ServiceTypes)
            {
                if (normalized.TryGetValue(serviceType, out var existing))
                {
                    merged.Add(existing);
                    normalized.Remove(serviceType);
                    continue;
                }

                merged.Add(new ServiceTypeStandard
                {
                    ServiceType = serviceType,
                    Rate = GetDefaultRate(serviceType),
                    Multiplier = 1m
                });
            }

            if (normalized.Count > 0)
                merged.AddRange(normalized.Values);

            return merged;
        }

        public static void Save(IEnumerable<ServiceTypeStandard> standards)
        {
            var path = GetStandardsPath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var payload = standards?
                .Where(item => !string.IsNullOrWhiteSpace(item.ServiceType))
                .Select(item => new ServiceTypeStandard
                {
                    ServiceType = item.ServiceType,
                    Rate = item.Rate,
                    Multiplier = item.Multiplier <= 0m ? 1m : item.Multiplier
                })
                .ToList() ?? new List<ServiceTypeStandard>();

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json);
        }

        private static List<ServiceTypeStandard> ReadStandardsFile()
        {
            var path = GetStandardsPath();
            if (!File.Exists(path))
                return GetDefaultStandards();

            try
            {
                var json = File.ReadAllText(path);
                var standards = JsonSerializer.Deserialize<List<ServiceTypeStandard>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return standards ?? GetDefaultStandards();
            }
            catch
            {
                return GetDefaultStandards();
            }
        }

        private static List<ServiceTypeStandard> GetDefaultStandards()
        {
            return new List<ServiceTypeStandard>
            {
                new ServiceTypeStandard { ServiceType = "Standard Clean", Rate = 0.12m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Initial Clean (Standard Clean Items Only)", Rate = 0.16m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Deep Clean (one-time)", Rate = 0.30m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Deep Clean (post initial clean)", Rate = 0.20m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Deep Clean (Semi-Annual)", Rate = 0.22m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Deep Clean (Annual)", Rate = 0.22m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Light Clean (dusting and floors)", Rate = 0.06m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Move In/Out Clean", Rate = 0.32m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Pressure Washing", Rate = 0m, Multiplier = 1m },
                new ServiceTypeStandard { ServiceType = "Organizing", Rate = 0m, Multiplier = 1m }
            };
        }

        private static decimal GetDefaultRate(string serviceType)
        {
            return GetDefaultStandards()
                .FirstOrDefault(item => string.Equals(item.ServiceType, serviceType, StringComparison.OrdinalIgnoreCase))
                ?.Rate ?? 0m;
        }

        private static string GetStandardsPath()
        {
            return Path.Combine(AppContext.BaseDirectory, "Data", StandardsFileName);
        }
    }
}

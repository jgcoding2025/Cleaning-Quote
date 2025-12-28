using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Cleaning_Quote.Data
{
    public class ServiceCatalog
    {
        public List<string> ServiceTypes { get; set; } = new List<string>();
        public List<string> SubItems { get; set; } = new List<string>();
    }

    public static class ServiceCatalogStore
    {
        private const string CatalogFileName = "ServiceCatalog.json";

        public static ServiceCatalog Load()
        {
            var path = GetCatalogPath();
            if (!File.Exists(path))
                return GetDefaultCatalog();

            try
            {
                var json = File.ReadAllText(path);
                var catalog = JsonSerializer.Deserialize<ServiceCatalog>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (catalog == null)
                    return GetDefaultCatalog();

                catalog.ServiceTypes ??= new List<string>();
                catalog.SubItems ??= new List<string>();
                return catalog;
            }
            catch
            {
                return GetDefaultCatalog();
            }
        }

        private static ServiceCatalog GetDefaultCatalog()
        {
            return new ServiceCatalog
            {
                ServiceTypes = new List<string>
                {
                    "Standard Clean",
                    "Initial Clean (Standard Clean Items Only)",
                    "Deep Clean (one-time)",
                    "Deep Clean (post initial clean)",
                    "Deep Clean (Semi-Annual)",
                    "Deep Clean (Annual)",
                    "Light Clean (dusting and floors)",
                    "Move In/Out Clean",
                    "Pressure Washing",
                    "Organizing"
                },
                SubItems = new List<string>
                {
                    "Ceiling Fan",
                    "Fridge",
                    "Mirror +1",
                    "Oven",
                    "Shower: Discount (no glass)",
                    "Shower: Discount (no stone/tile)",
                    "Sink: Discount 1",
                    "Stove Top (Gas)",
                    "Tub / Jacuzzi Tub",
                    "Window (1 pane, inside only, 1st story)",
                    "Window (1 pane, outside only, 1st story)",
                    "Window (1 pane, inside only, 2nd story)",
                    "Window (1 pane, outside only, 2nd story)",
                    "Window Tract",
                    "Window: Standard (2 panes)"
                }
            };
        }

        private static string GetCatalogPath()
        {
            return Path.Combine(AppContext.BaseDirectory, "Data", CatalogFileName);
        }
    }
}

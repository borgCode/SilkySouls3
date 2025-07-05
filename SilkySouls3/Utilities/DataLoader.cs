using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SilkySouls3.Models;

namespace SilkySouls3.Utilities
{
    public static class DataLoader
    {
        public static Dictionary<string, List<WarpLocation>> GetWarpLocations()
        {
            Dictionary<string, List<WarpLocation>> warpDict = new Dictionary<string, List<WarpLocation>>();
            string csvData = Properties.Resources.WarpEntries;

            if (string.IsNullOrWhiteSpace(csvData))
                return warpDict;

            using (StringReader reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    if (!GameLauncher.IsDlc2Available && line.StartsWith("2,"))
                        continue;

                    string[] parts = line.Split(',');
                    if (parts.Length < 6) continue;

                    string mainArea = parts[1];
                    string name = parts[2];

                    string offsetStr = parts[3];
                    int? offset = null;

                    if (!string.IsNullOrWhiteSpace(offsetStr))
                    {
                        if (offsetStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                            offsetStr = offsetStr.Substring(2);

                        if (int.TryParse(offsetStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var o))
                            offset = o;
                    }

                    byte? bitPos = null;
                    if (!string.IsNullOrWhiteSpace(parts[4]))
                    {
                        bitPos = byte.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var b)
                            ? (byte?)b
                            : null;
                    }

                    int bonfireId = int.Parse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture);

                    float[] coords = null;
                    float angle = 0f;

                    if (parts.Length > 6 && !string.IsNullOrWhiteSpace(parts[6]))
                    {
                        var coordParts = parts[6].Split('|');
                        coords = new float[coordParts.Length];
                        for (int i = 0; i < coordParts.Length; i++)
                        {
                            coords[i] = float.Parse(coordParts[i], CultureInfo.InvariantCulture);
                        }
                    }

                    if (parts.Length > 7 && !string.IsNullOrWhiteSpace(parts[7]))
                    {
                        angle = float.Parse(parts[7], CultureInfo.InvariantCulture);
                    }

                    WarpLocation location = new WarpLocation
                    {
                        Name = name,
                        MainArea = mainArea,
                        Offset = offset,
                        BitPosition = bitPos,
                        BonfireId = bonfireId,
                        Coords = coords,
                        Angle = angle
                    };

                    if (!warpDict.ContainsKey(mainArea))
                    {
                        warpDict[mainArea] = new List<WarpLocation>();
                    }

                    warpDict[mainArea].Add(location);
                }
            }

            return warpDict;
        }

        public static List<Item> GetItemList(string listName)
        {
            List<Item> items = new List<Item>();

            string csvData = Properties.Resources.ResourceManager.GetString(listName);

            if (string.IsNullOrEmpty(csvData))
            {
                return new List<Item>();
            }

            using (StringReader reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (!GameLauncher.IsDlc2Available && line.StartsWith("2,"))
                        continue;

                    string[] parts = line.Split(',');
                    if (parts.Length >= 6)
                    {
                        items.Add(new Item
                        {
                            Id = int.Parse(parts[1], NumberStyles.HexNumber),
                            Name = parts[2],
                            StackSize = int.Parse(parts[3]),
                            UpgradeType = int.Parse(parts[4]),
                            Infusable = parts[5] == "1"
                        });
                    }
                }
            }

            return items;
        }

        public static void SaveCustomLoadouts(Dictionary<string, LoadoutTemplate> customLoadouts)
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SilkySouls3");

            Directory.CreateDirectory(appDataPath);
            string filePath = Path.Combine(appDataPath, "CustomLoadouts.csv");

            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var loadout in customLoadouts.Values)
                    {
                        if (!string.IsNullOrEmpty(loadout.Name))
                        {
                            writer.WriteLine($"LOADOUT,{loadout.Name}");

                            foreach (var item in loadout.Items)
                            {
                                writer.WriteLine(
                                    $"ITEM,{item.ItemName},{item.Infusion},{item.Upgrade},{item.Quantity}");
                            }

                            writer.WriteLine("END");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Log exception if needed
            }
        }

        public static Dictionary<string, LoadoutTemplate> LoadCustomLoadouts()
        {
            Dictionary<string, LoadoutTemplate> customLoadouts = new Dictionary<string, LoadoutTemplate>();

            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SilkySouls3");

            string filePath = Path.Combine(appDataPath, "CustomLoadouts.csv");

            if (!File.Exists(filePath))
                return customLoadouts;

            try
            {
                LoadoutTemplate currentLoadout = null;

                using (var reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(',');

                        if (parts[0] == "LOADOUT" && parts.Length > 1)
                        {
                            currentLoadout = new LoadoutTemplate
                            {
                                Name = parts[1],
                                Items = new List<ItemTemplate>()
                            };
                        }
                        else if (parts[0] == "ITEM" && currentLoadout != null)
                        {
                            int quantity = 1;

                            if (parts.Length > 4)
                            {
                                int.TryParse(parts[4], out quantity);
                            }

                            currentLoadout.Items.Add(new ItemTemplate
                            {
                                ItemName = parts[1],
                                Infusion = parts[2],
                                Upgrade = int.Parse(parts[3]),
                                Quantity = quantity
                            });
                        }
                        else if (parts[0] == "END" && currentLoadout != null)
                        {
                            customLoadouts[currentLoadout.Name] = currentLoadout;
                            currentLoadout = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ignored
            }

            return customLoadouts;
        }
    }
}
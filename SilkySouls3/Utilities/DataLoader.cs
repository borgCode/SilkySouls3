using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SilkySouls3.Models;

namespace SilkySouls3.Utilities
{
    public static class DataLoader
    {
        public static Dictionary<string, WarpEntry> GetWarpEntryDict()
        {
            Dictionary<string, WarpEntry> entries = new Dictionary<string, WarpEntry>();
            string csvData = Properties.Resources.WarpEntries;

            if (string.IsNullOrWhiteSpace(csvData))
                return entries;

            using (StringReader reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    string[] parts = line.Split(',');
                    if (parts.Length < 4) continue;

                    string name = parts[0];

                    string offsetStr = parts[1];
                    int? offset = null;

                    if (offsetStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        offsetStr = offsetStr.Substring(2);

                    if (int.TryParse(offsetStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var o))
                        offset = o;


                    byte? bitPos = byte.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out var b)
                        ? (byte?)b
                        : null;

                    int bonfireId = int.Parse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture);

                    float[] coords = null;
                    float angle = 0f;

                    if (parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]))
                    {
                        var coordParts = parts[4].Split('|');
                        coords = new float[coordParts.Length];
                        for (int i = 0; i < coordParts.Length; i++)
                        {
                            coords[i] = float.Parse(coordParts[i], CultureInfo.InvariantCulture);
                        }
                    }

                    if (parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]))
                    {
                        angle = float.Parse(parts[5], CultureInfo.InvariantCulture);
                    }

                    string key = $"{bonfireId},{name}";
                    entries.Add(key, new WarpEntry
                    {
                        Name = name,
                        Offset = offset,
                        BitPosition = bitPos,
                        BonfireID = bonfireId,
                        Coords = coords,
                        Angle = angle
                    });
                }
            }

            return entries;
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

                    string[] parts = line.Split(',');
                    if (parts.Length >= 5)
                    {
                        items.Add(new Item
                        {
                            Id = int.Parse(parts[0], NumberStyles.HexNumber),
                            Name = parts[1],
                            StackSize = int.Parse(parts[2]),
                            UpgradeType = int.Parse(parts[3]),
                            Infusable = parts[4] == "1"
                        });
                    }
                }
            }

            return items;
        }
    }
}
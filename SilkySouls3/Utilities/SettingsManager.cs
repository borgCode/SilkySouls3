using System;
using System.IO;

namespace SilkySouls3.Utilities
{
    public class SettingsManager
    {
        private static SettingsManager _default;
        public static SettingsManager Default => _default ?? (_default = Load());

        public string HotkeyActionIds { get; set; } = "";
        public string HotkeyValues { get; set; } = "";
        public bool EnableHotkeys { get; set; }
        public bool StutterFix { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool DisableMenuMusic { get; set; }
        public bool HotkeyReminder { get; set; }
        public bool DefaultSoundChangeEnabled { get; set; }
        public int DefaultSoundVolume { get; set; } = 3;
        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }
        public double ResistancesWindowScaleX { get; set; } = 1.0;
        public double ResistancesWindowScaleY { get; set; } = 1.0;
        public double ResistancesWindowWidth { get; set; }
        public double ResistancesWindowLeft { get; set; }
        public double ResistancesWindowTop { get; set; }
        public bool EnableUpdateChecks { get; set; } = true;

        private static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SilkySouls3",
            "settings.json");

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                
                var lines = new[]
                {
                    $"HotkeyActionIds={HotkeyActionIds}",
                    $"HotkeyValues={HotkeyValues}",
                    $"EnableHotkeys={EnableHotkeys}",
                    $"StutterFix={StutterFix}",
                    $"AlwaysOnTop={AlwaysOnTop}",
                    $"DisableMenuMusic={DisableMenuMusic}",
                    $"HotkeyReminder={HotkeyReminder}",
                    $"DefaultSoundChangeEnabled={DefaultSoundChangeEnabled}",
                    $"DefaultSoundVolume={DefaultSoundVolume}",
                    $"WindowLeft={WindowLeft}",
                    $"WindowTop={WindowTop}",
                    $"ResistancesWindowScaleX={ResistancesWindowScaleX}",
                    $"ResistancesWindowScaleY={ResistancesWindowScaleY}",
                    $"ResistancesWindowWidth={ResistancesWindowWidth}",
                    $"ResistancesWindowLeft={ResistancesWindowLeft}",
                    $"ResistancesWindowTop={ResistancesWindowTop}",
                    $"EnableUpdateChecks={EnableUpdateChecks}",
                };

                File.WriteAllLines(SettingsPath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private static SettingsManager Load()
        {
            var settings = new SettingsManager();

            if (File.Exists(SettingsPath))
            {
                try
                {
                    foreach (var line in File.ReadAllLines(SettingsPath))
                    {
                        var parts = line.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0];
                            var value = parts[1];

                            switch (key)
                            {
                                case "HotkeyActionIds": settings.HotkeyActionIds = value; break;
                                case "HotkeyValues": settings.HotkeyValues = value; break;
                                case "EnableHotkeys":
                                    bool.TryParse(value, out bool eh);
                                    settings.EnableHotkeys = eh;
                                    break;
                                case "StutterFix":
                                    bool.TryParse(value, out bool sf);
                                    settings.StutterFix = sf;
                                    break;
                                case "AlwaysOnTop":
                                    bool.TryParse(value, out bool aot);
                                    settings.AlwaysOnTop = aot;
                                    break;
                                case "DisableMenuMusic":
                                    bool.TryParse(value, out bool dmm);
                                    settings.DisableMenuMusic = dmm;
                                    break;
                                case "HotkeyReminder":
                                    bool.TryParse(value, out bool hr);
                                    settings.HotkeyReminder = hr;
                                    break;
                                case "DefaultSoundChangeEnabled":
                                    bool.TryParse(value, out bool dsce);
                                    settings.DefaultSoundChangeEnabled = dsce;
                                    break;
                                case "DefaultSoundVolume":
                                    int.TryParse(value, out int dsv);
                                    settings.DefaultSoundVolume = dsv;
                                    break;
                                case "WindowLeft":
                                    double.TryParse(value, out double wl);
                                    settings.WindowLeft = wl;
                                    break;
                                case "WindowTop":
                                    double.TryParse(value, out double wt);
                                    settings.WindowTop = wt;
                                    break;
                                case "ResistancesWindowScaleX":
                                    double.TryParse(value, out double rwx);
                                    settings.ResistancesWindowScaleX = rwx;
                                    break;
                                case "ResistancesWindowScaleY":
                                    double.TryParse(value, out double rwy);
                                    settings.ResistancesWindowScaleY = rwy;
                                    break;
                                case "ResistancesWindowLeft":
                                    double.TryParse(value, out double rwl);
                                    settings.ResistancesWindowLeft = rwl;
                                    break;
                                case "ResistancesWindowTop":
                                    double.TryParse(value, out double rwt);
                                    settings.ResistancesWindowTop = rwt;
                                    break;
                                case "ResistancesWindowWidth":
                                    double.TryParse(value, out double rww);
                                    settings.ResistancesWindowWidth = rww;
                                    break;
                                case "EnableUpdateChecks":
                                    bool.TryParse(value, out bool euc);
                                    settings.EnableUpdateChecks = euc;
                                    break;
                            }
                        }
                    }
                }
                catch
                {
                    // Return default settings on error
                }
            }

            return settings;
        }
    }
}
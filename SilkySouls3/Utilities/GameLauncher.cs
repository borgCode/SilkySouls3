using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using SilkySouls3.Memory;

namespace SilkySouls3.Utilities
{
    public static class GameLauncher
    {
        
        public static void LaunchDarkSouls3()
        {
            try
            {
                string exePath = GetDarkSouls3ExePath();
                if (exePath == null)
                {
                    return;
                }
                var process = new Process { StartInfo = new ProcessStartInfo(exePath) };
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch Dark Souls III: {ex.Message}", 
                    "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public static void SetVersionOffsets()
        {
            try
            {
                string exePath = GetDarkSouls3ExePath();
                if (exePath == null)
                {
                    return;
                }
        
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                
                int major = versionInfo.FileMajorPart;
                int minor = versionInfo.FileMinorPart;
                
                if (major == 1 && minor <= 12)
                {
                    Offsets.WorldChrMan.PlayerInsOffsets.CharFlags1 = 0x1EE0;
                    Offsets.WorldChrMan.PlayerInsOffsets.Modules = 0x1F88;
                }
                else
                {
                    Offsets.WorldChrMan.PlayerInsOffsets.CharFlags1 = 0x1EE8;
                    Offsets.WorldChrMan.PlayerInsOffsets.Modules = 0x1F90;
                }
            }
            catch (Exception)
            {
                Offsets.WorldChrMan.PlayerInsOffsets.CharFlags1 = 0x1EE8;
                Offsets.WorldChrMan.PlayerInsOffsets.Modules = 0x1F90;
            }
        }
        
        private static string GetDarkSouls3ExePath()
        {
            try
            {
                string steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string;
                if (string.IsNullOrEmpty(steamPath))
                    throw new FileNotFoundException("Steam installation path not found in registry.");
        
                string configPath = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
                if (!File.Exists(configPath))
                    throw new FileNotFoundException($"Steam library configuration not found at {configPath}");
        
                var paths = new List<string> { steamPath };
                var regex = new Regex(@"""path""\s*""(.+?)""");
        
                foreach (var line in File.ReadLines(configPath))
                {
                    var match = regex.Match(line);
                    if (match.Success) paths.Add(match.Groups[1].Value.Replace(@"\\", @"\"));
                }
        
                foreach (var path in paths)
                {
                    string fullPath = Path.Combine(path, @"steamapps\common\DARK SOULS III\Game\DarkSoulsIII.exe");
                    if (File.Exists(fullPath)) return fullPath;
                }
        
                throw new FileNotFoundException("Dark Souls III executable not found in any Steam library.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding Dark Souls III: {ex.Message}", "Game Not Found",MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                return null;
            }
        }
    }
}
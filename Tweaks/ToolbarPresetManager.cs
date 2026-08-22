using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Platform;
using OsuTweaks.Models;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Управляет JSON-пресетами расположения тулбара, сохраняя их через Storage плагина (presets/*.json).
    /// </summary>
    public static class ToolbarPresetManager
    {
        private static Storage? pluginStorage;
        public static string PresetsDirectory => pluginStorage != null 
            ? pluginStorage.GetFullPath("presets") 
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu", "osu-cc", "plugins", "osu-tweaks", "presets");

        public static void Init(Storage storage)
        {
            pluginStorage = storage;
            EnsureDefaultPresetsExist();
        }

        public static void EnsureDefaultPresetsExist()
        {
            try
            {
                if (!Directory.Exists(PresetsDirectory))
                    Directory.CreateDirectory(PresetsDirectory);

                string defaultPath = Path.Combine(PresetsDirectory, "Default (Ванильный).json");
                if (!File.Exists(defaultPath))
                {
                    ToolbarLayoutConfig.CreateDefault().Save(defaultPath);
                }

                string centeredPath = Path.Combine(PresetsDirectory, "Centered (По центру).json");
                if (!File.Exists(centeredPath))
                {
                    ToolbarLayoutConfig.CreateCentered().Save(centeredPath);
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("Failed to ensure default presets exist", ex);
            }
        }

        public static List<string> GetAvailablePresets()
        {
            EnsureDefaultPresetsExist();
            var list = new List<string>();

            try
            {
                if (Directory.Exists(PresetsDirectory))
                {
                    foreach (var file in Directory.GetFiles(PresetsDirectory, "*.json"))
                    {
                        list.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("Failed to list presets", ex);
            }

            if (list.Count == 0)
            {
                list.Add("Default (Ванильный)");
            }

            return list;
        }

        public static ToolbarLayoutConfig LoadPreset(string presetName)
        {
            EnsureDefaultPresetsExist();
            string path = Path.Combine(PresetsDirectory, presetName + ".json");
            if (File.Exists(path))
            {
                return ToolbarLayoutConfig.Load(path);
            }

            return ToolbarLayoutConfig.CreateDefault();
        }

        public static void SaveCustomPreset(string presetName, ToolbarLayoutConfig config)
        {
            EnsureDefaultPresetsExist();
            string path = Path.Combine(PresetsDirectory, presetName + ".json");
            config.Save(path);
        }
    }
}

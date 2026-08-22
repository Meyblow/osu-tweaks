using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using osucc.Data;
using OsuTweaks.Models;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Управляет JSON-пресетами расположения тулбара, используя IOsuCcStorage плагина (presets/*.json).
    /// </summary>
    public static class ToolbarPresetManager
    {
        private static IOsuCcStorage? storage;

        public static void Init(IOsuCcStorage pluginStorage)
        {
            storage = pluginStorage;
            EnsureDefaultPresetsExist();
        }

        public static void EnsureDefaultPresetsExist()
        {
            if (storage == null) return;

            try
            {
                if (!storage.Exists("presets/Default (Ванильный).json"))
                {
                    storage.WriteJson("presets/Default (Ванильный).json", ToolbarLayoutConfig.CreateDefault());
                }

                if (!storage.Exists("presets/Centered (По центру).json"))
                {
                    storage.WriteJson("presets/Centered (По центру).json", ToolbarLayoutConfig.CreateCentered());
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("Failed to ensure default presets exist via IOsuCcStorage", ex);
            }
        }

        public static List<string> GetAvailablePresets()
        {
            EnsureDefaultPresetsExist();
            var list = new List<string>();

            if (storage != null)
            {
                try
                {
                    var files = storage.GetFiles("presets", "*.json");
                    foreach (var file in files)
                    {
                        list.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
                catch (Exception ex)
                {
                    TweaksLog.Error("Failed to list presets from IOsuCcStorage", ex);
                }
            }

            if (list.Count == 0)
            {
                list.Add("Default (Ванильный)");
                list.Add("Centered (По центру)");
            }

            return list;
        }

        public static ToolbarLayoutConfig LoadPreset(string presetName)
        {
            EnsureDefaultPresetsExist();

            if (storage != null)
            {
                try
                {
                    string relativePath = $"presets/{presetName}.json";
                    if (storage.Exists(relativePath))
                    {
                        var loaded = storage.ReadJson<ToolbarLayoutConfig>(relativePath);
                        if (loaded != null) return loaded;
                    }
                }
                catch (Exception ex)
                {
                    TweaksLog.Error($"Failed to load preset '{presetName}' via IOsuCcStorage", ex);
                }
            }

            return ToolbarLayoutConfig.CreateDefault();
        }

        public static void SaveCustomPreset(string presetName, ToolbarLayoutConfig config)
        {
            if (storage == null) return;

            try
            {
                string relativePath = $"presets/{presetName}.json";
                storage.WriteJson(relativePath, config);
                TweaksLog.Info($"SaveCustomPreset: Saved preset '{presetName}' to {relativePath}");
            }
            catch (Exception ex)
            {
                TweaksLog.Error($"Failed to save preset '{presetName}' via IOsuCcStorage", ex);
            }
        }

        public static bool DeletePreset(string presetName)
        {
            if (storage == null) return false;

            try
            {
                string? fullPath = storage.GetFullPath($"presets/{presetName}.json");
                if (fullPath != null && File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    TweaksLog.Info($"DeletePreset: Deleted preset '{presetName}'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error($"Failed to delete preset '{presetName}'", ex);
            }

            return false;
        }

        public static void OpenPresetsFolder()
        {
            if (storage == null) return;

            try
            {
                string? fullPath = storage.GetFullPath("presets");
                if (fullPath != null)
                {
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("Failed to open presets folder", ex);
            }
        }
    }
}

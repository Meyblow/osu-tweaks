using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using osucc.Data;
using OsuTweaks.Models;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Управляет JSON-пресетами расположения тулбара, используя IOsuCcStorage плагина (presets/*.json)
    /// или безопасный fallback на локальную директорию плагина.
    /// </summary>
    public static class ToolbarPresetManager
    {
        private static IOsuCcStorage? storage;

        private static string FallbackPresetsDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu", "osu-cc", "plugins", "osu-tweaks", "presets");

        public static void Init(IOsuCcStorage? pluginStorage)
        {
            storage = pluginStorage;
            EnsureDefaultPresetsExist();
        }

        public static void EnsureDefaultPresetsExist()
        {
            try
            {
                if (storage != null)
                {
                    if (!storage.Exists("presets/Default (Ванильный).json"))
                        storage.WriteJson("presets/Default (Ванильный).json", ToolbarLayoutConfig.CreateDefault());

                    if (!storage.Exists("presets/Centered (По центру).json"))
                        storage.WriteJson("presets/Centered (По центру).json", ToolbarLayoutConfig.CreateCentered());
                }
                else
                {
                    if (!Directory.Exists(FallbackPresetsDirectory))
                        Directory.CreateDirectory(FallbackPresetsDirectory);

                    string def = Path.Combine(FallbackPresetsDirectory, "Default (Ванильный).json");
                    if (!File.Exists(def))
                        ToolbarLayoutConfig.CreateDefault().Save(def);

                    string cent = Path.Combine(FallbackPresetsDirectory, "Centered (По центру).json");
                    if (!File.Exists(cent))
                        ToolbarLayoutConfig.CreateCentered().Save(cent);
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
                if (storage != null)
                {
                    var files = storage.GetFiles("presets", "*.json");
                    foreach (var file in files)
                    {
                        list.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
                else if (Directory.Exists(FallbackPresetsDirectory))
                {
                    foreach (var file in Directory.GetFiles(FallbackPresetsDirectory, "*.json"))
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
                list.Add("Centered (По центру)");
            }

            return list;
        }

        public static ToolbarLayoutConfig LoadPreset(string presetName)
        {
            EnsureDefaultPresetsExist();

            try
            {
                if (storage != null)
                {
                    string relativePath = $"presets/{presetName}.json";
                    if (storage.Exists(relativePath))
                    {
                        var loaded = storage.ReadJson<ToolbarLayoutConfig>(relativePath);
                        if (loaded != null) return loaded;
                    }
                }
                else
                {
                    string fullPath = Path.Combine(FallbackPresetsDirectory, $"{presetName}.json");
                    if (File.Exists(fullPath))
                        return ToolbarLayoutConfig.Load(fullPath);
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error($"Failed to load preset '{presetName}'", ex);
            }

            return ToolbarLayoutConfig.CreateDefault();
        }

        public static void SaveCustomPreset(string presetName, ToolbarLayoutConfig config)
        {
            try
            {
                if (storage != null)
                {
                    string relativePath = $"presets/{presetName}.json";
                    storage.WriteJson(relativePath, config);
                }
                else
                {
                    if (!Directory.Exists(FallbackPresetsDirectory))
                        Directory.CreateDirectory(FallbackPresetsDirectory);

                    string fullPath = Path.Combine(FallbackPresetsDirectory, $"{presetName}.json");
                    config.Save(fullPath);
                }
                TweaksLog.Info($"SaveCustomPreset: Saved preset '{presetName}'");
            }
            catch (Exception ex)
            {
                TweaksLog.Error($"Failed to save preset '{presetName}'", ex);
            }
        }

        public static bool DeletePreset(string presetName)
        {
            try
            {
                string? fullPath = storage?.GetFullPath($"presets/{presetName}.json")
                    ?? Path.Combine(FallbackPresetsDirectory, $"{presetName}.json");

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
            try
            {
                string? fullPath = storage?.GetFullPath("presets") ?? FallbackPresetsDirectory;
                if (fullPath != null)
                {
                    if (!Directory.Exists(fullPath))
                        Directory.CreateDirectory(fullPath);

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

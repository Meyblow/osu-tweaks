using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OsuTweaks.Models
{
    public class ToolbarItemConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("hidden")]
        public bool IsHidden { get; set; }

        [JsonPropertyName("width")]
        public float? CustomWidth { get; set; }
    }

    public class ToolbarLayoutConfig
    {
        private static readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true };

        [JsonPropertyName("left")]
        public List<ToolbarItemConfig> Left { get; set; } = new();

        [JsonPropertyName("center")]
        public List<ToolbarItemConfig> Center { get; set; } = new();

        [JsonPropertyName("right")]
        public List<ToolbarItemConfig> Right { get; set; } = new();

        /// <summary>
        /// 100% ванильный стандартный тулбар osu!
        /// </summary>
        public static ToolbarLayoutConfig CreateDefault()
        {
            return new ToolbarLayoutConfig
            {
                Left = new List<ToolbarItemConfig>
                {
                    new() { Id = "settings" },
                    new() { Id = "home" },
                    new() { Id = "rulesets" }
                },
                Center = new List<ToolbarItemConfig>(),
                Right = new List<ToolbarItemConfig>
                {
                    new() { Id = "rankings" },
                    new() { Id = "news" },
                    new() { Id = "changelog" },
                    new() { Id = "wiki" },
                    new() { Id = "beatmap_listing" },
                    new() { Id = "chat" },
                    new() { Id = "social" },
                    new() { Id = "music" },
                    new() { Id = "user_profile" },
                    new() { Id = "clock" },
                    new() { Id = "notifications" }
                }
            };
        }

        /// <summary>
        /// Пресет с центрированным блоком профиля, музыки и часов
        /// </summary>
        public static ToolbarLayoutConfig CreateCentered()
        {
            return new ToolbarLayoutConfig
            {
                Left = new List<ToolbarItemConfig>
                {
                    new() { Id = "settings" },
                    new() { Id = "home" },
                    new() { Id = "rulesets" }
                },
                Center = new List<ToolbarItemConfig>
                {
                    new() { Id = "clock" },
                    new() { Id = "user_profile" },
                    new() { Id = "music" }
                },
                Right = new List<ToolbarItemConfig>
                {
                    new() { Id = "chat" },
                    new() { Id = "social" },
                    new() { Id = "notifications" },
                    new() { Id = "rankings" },
                    new() { Id = "news" },
                    new() { Id = "changelog" },
                    new() { Id = "wiki" },
                    new() { Id = "beatmap_listing" }
                }
            };
        }

        public static ToolbarLayoutConfig Load(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var config = JsonSerializer.Deserialize<ToolbarLayoutConfig>(json);
                    if (config != null)
                        return config;
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error($"Failed to load layout from {filePath}, using default", ex);
            }

            return CreateDefault();
        }

        public void Save(string filePath)
        {
            try
            {
                string dir = Path.GetDirectoryName(filePath)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(this, serializerOptions);
                File.WriteAllText(filePath, json);
                TweaksLog.Info($"Toolbar layout successfully saved to {filePath}");
            }
            catch (Exception ex)
            {
                TweaksLog.Error($"Failed to save layout to {filePath}", ex);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на GameplayMenuOverlay.PopIn для отключения резкого звука проигрыша карты при открытии FailOverlay.
    /// Поддерживает обратимость: восстанавливает исходную громкость при деактивации или закрытии.
    /// </summary>
    public sealed class FailSoundPatch : PluginPatch<OsuTweaksPlugin>
    {
        private static Bindable<bool>? silentFailBindable;
        private static readonly Dictionary<SkinnableSound, double> originalSkinnableVolumes = new();
        private static readonly Dictionary<ISample, double> originalSampleVolumes = new();

        public FailSoundPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<bool> silentFail)
            : base(plugin, host, "osu.Game.Screens.Play.GameplayMenuOverlay", "PopIn", MethodType.Prefix)
        {
            silentFailBindable = silentFail;
        }

        public static void Prefix(GameplayMenuOverlay __instance)
        {
            if (__instance is not FailOverlay || silentFailBindable?.Value != true)
                return;

            try
            {
                // Запоминаем исходную громкость и глушим звук фейла
                foreach (var skinnable in __instance.ChildrenOfType<SkinnableSound>())
                {
                    if (!originalSkinnableVolumes.ContainsKey(skinnable))
                    {
                        originalSkinnableVolumes[skinnable] = skinnable.Volume.Value;
                    }
                    skinnable.Volume.Value = 0;
                }

                var sampleField = ReflectionHelper.FindField(__instance.GetType(), "failSample")
                                  ?? ReflectionHelper.FindField(__instance.GetType(), "sample");

                if (sampleField != null && sampleField.GetValue(__instance) is ISample s)
                {
                    if (!originalSampleVolumes.ContainsKey(s))
                    {
                        originalSampleVolumes[s] = s.Volume.Value;
                    }
                    s.Volume.Value = 0;
                }

                TweaksLog.Info("FailSoundPatch: Silenced fail sound on death!");
            }
            catch (Exception ex)
            {
                TweaksLog.Error("FailSoundPatch error", ex);
            }
        }

        public static void RestoreOriginalVolumes()
        {
            try
            {
                foreach (var kvp in originalSkinnableVolumes)
                {
                    kvp.Key.Volume.Value = kvp.Value;
                }
                originalSkinnableVolumes.Clear();

                foreach (var kvp in originalSampleVolumes)
                {
                    kvp.Key.Volume.Value = kvp.Value;
                }
                originalSampleVolumes.Clear();
            }
            catch { }
        }
    }
}

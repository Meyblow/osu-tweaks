using System;
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
    /// </summary>
    public sealed class FailSoundPatch : PluginPatch<OsuTweaksPlugin>
    {
        private static Bindable<bool>? silentFailBindable;

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
                // Глушим звук фейла внутри FailOverlay
                foreach (var skinnable in __instance.ChildrenOfType<SkinnableSound>())
                {
                    skinnable.Volume.Value = 0;
                }

                var sampleField = ReflectionHelper.FindField(__instance.GetType(), "failSample")
                                  ?? ReflectionHelper.FindField(__instance.GetType(), "sample");

                if (sampleField != null && sampleField.GetValue(__instance) is ISample s)
                {
                    s.Volume.Value = 0;
                }

                TweaksLog.Info("FailSoundPatch: Silenced fail sound on death!");
            }
            catch (Exception ex)
            {
                TweaksLog.Error("FailSoundPatch error", ex);
            }
        }
    }
}

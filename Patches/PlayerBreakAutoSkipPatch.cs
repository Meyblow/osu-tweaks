using System;
using osu.Game.Screens.Play;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Tweaks;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на Player.LoadComplete для добавления TweaksBreakAutoSkipper в GameplayClockContainer.
    /// </summary>
    public sealed class PlayerBreakAutoSkipPatch : PluginPatch<OsuTweaksPlugin>
    {
        public PlayerBreakAutoSkipPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Screens.Play.Player", "LoadComplete", MethodType.Postfix)
        {
        }

        public static void Postfix(Player __instance)
        {
            if (__instance == null) return;
            try
            {
                var plugin = OsuTweaksPlugin.Instance;
                if (plugin == null || !plugin.AutoSkipBreaks.Value)
                    return;

                var breaks = __instance.GameplayState?.Beatmap?.Breaks;
                if (breaks == null || breaks.Count == 0)
                    return;

                var gcc = ReflectionHelper.GetPropertyValue<GameplayClockContainer>(__instance, "GameplayClockContainer");
                if (gcc != null)
                {
                    var skipper = new TweaksBreakAutoSkipper(__instance, gcc, breaks);
                    gcc.Add(skipper);
                    TweaksLog.Info($"PlayerBreakAutoSkipPatch: Successfully injected TweaksBreakAutoSkipper ({breaks.Count} breaks)");
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("PlayerBreakAutoSkipPatch failed to inject skipper", ex);
            }
        }
    }
}

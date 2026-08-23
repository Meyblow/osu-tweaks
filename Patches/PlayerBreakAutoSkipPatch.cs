using System;
using osu.Game.Screens.Play;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Models;
using OsuTweaks.Tweaks;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на Player.LoadComplete для внедрения TweaksBreakAutoSkipper в GameplayClockContainer.
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
                if (plugin == null || plugin.AutoSkipMode.Value == AutoSkipMode.Disabled)
                    return;

                var gcc = ReflectionHelper.GetPropertyValue<GameplayClockContainer>(__instance, "GameplayClockContainer");
                if (gcc != null)
                {
                    var skipper = new TweaksIntroOutroSkipper(__instance, gcc);
                    gcc.Add(skipper);
                    TweaksLog.Info($"PlayerBreakAutoSkipPatch: Successfully injected TweaksIntroOutroSkipper (Mode={plugin.AutoSkipMode.Value})");
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("PlayerBreakAutoSkipPatch failed to inject skipper", ex);
            }
        }
    }
}

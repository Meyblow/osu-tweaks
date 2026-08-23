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
    /// Патч на Player.LoadComplete для внедрения геймплейных твиков (TweaksAutoSkipper, TweaksHUDCustomizer, TweaksScreenShakeCustomizer).
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
                if (plugin == null) return;

                var gcc = ReflectionHelper.GetPropertyValue<GameplayClockContainer>(__instance, "GameplayClockContainer");
                if (gcc != null)
                {
                    // 1. Внедряем автоскиппер в ClockContainer
                    if (plugin.AutoSkipMode.Value != AutoSkipMode.Disabled)
                    {
                        var skipper = new TweaksAutoSkipper(__instance, gcc);
                        gcc.Add(skipper);
                        TweaksLog.Info($"PlayerBreakAutoSkipPatch: Successfully injected TweaksAutoSkipper (Mode={plugin.AutoSkipMode.Value})");
                    }

                    // 2. Внедряем Minimalist HUD кастомайзер
                    var hudCustomizer = new TweaksHUDCustomizer(__instance, gcc);
                    gcc.Add(hudCustomizer);

                    // 3. Внедряем Screen Shake кастомайзер
                    var shakeCustomizer = new TweaksScreenShakeCustomizer(__instance);
                    gcc.Add(shakeCustomizer);
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("PlayerBreakAutoSkipPatch error during component injection", ex);
            }
        }
    }
}

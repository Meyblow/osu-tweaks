using System;
using osu.Framework.Graphics;
using osu.Game.Screens.Play;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на Player.OnExiting для мгновенного завершения анимации ухода старого игрока при рестарте.
    /// </summary>
    public sealed class PlayerExitingRestartPatch : PluginPatch<OsuTweaksPlugin>
    {
        public PlayerExitingRestartPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Screens.Play.Player", "OnExiting", MethodType.Prefix)
        {
        }

        public static void Prefix(Player __instance)
        {
            if (__instance == null || !InstantRestartPatch.IsRestarting)
                return;

            try
            {
                __instance.FinishTransforms(true);
                __instance.Alpha = 0f;
            }
            catch (Exception ex)
            {
                TweaksLog.Error("PlayerExitingRestartPatch error", ex);
            }
        }
    }
}

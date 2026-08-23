using System;
using osu.Framework.Bindables;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на Player.Restart для фиксации намерения мгновенного перезапуска карты.
    /// </summary>
    public sealed class InstantRestartPatch : PluginPatch<OsuTweaksPlugin>
    {
        private static Bindable<bool>? instantRetryBindable;
        public static bool IsRestarting { get; set; }

        public InstantRestartPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<bool> instantRetry)
            : base(plugin, host, "osu.Game.Screens.Play.Player", "Restart", MethodType.Prefix)
        {
            instantRetryBindable = instantRetry;
        }

        public static void Prefix()
        {
            if (instantRetryBindable?.Value == true)
            {
                IsRestarting = true;
                TweaksLog.Info("InstantRestartPatch: Player.Restart triggered - fast restart enabled!");
            }
        }
    }
}

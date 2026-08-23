using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Play;
using osucc.Core;
using osucc.Plugin;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на PlayerLoader.OnEntering для мгновенного пропуска задержек загрузчика при рестарте карты.
    /// </summary>
    public sealed class InstantRestartPatch : PluginPatch<OsuTweaksPlugin>
    {
        private static Bindable<bool>? instantRetryBindable;

        public InstantRestartPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<bool> instantRetry)
            : base(plugin, host, "osu.Game.Screens.Play.PlayerLoader", "OnEntering", MethodType.Postfix)
        {
            instantRetryBindable = instantRetry;
        }

        public static void Postfix(PlayerLoader __instance)
        {
            if (__instance == null || instantRetryBindable?.Value != true)
                return;

            try
            {
                // Завершаем трансформации экрана загрузки мгновенно для мгновенного входа в игру
                __instance.FinishTransforms(true);
                TweaksLog.Info("InstantRestartPatch: Fast-forwarded PlayerLoader entrance for zero-delay restart!");
            }
            catch (Exception ex)
            {
                TweaksLog.Error("InstantRestartPatch error", ex);
            }
        }
    }
}

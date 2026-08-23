using System;
using osu.Framework.Graphics;
using osu.Game.Screens.Play;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на PlayerLoader.OnResuming для сброса задержек загрузчика при рестарте.
    /// </summary>
    public sealed class PlayerLoaderResumingPatch : PluginPatch<OsuTweaksPlugin>
    {
        public PlayerLoaderResumingPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Screens.Play.PlayerLoader", "OnResuming", MethodType.Postfix)
        {
        }

        public static void Postfix(PlayerLoader __instance)
        {
            if (__instance == null || !InstantRestartPatch.IsRestarting)
                return;

            try
            {
                __instance.FinishTransforms(true);
                TweaksLog.Info("PlayerLoaderResumingPatch: Fast-forwarded PlayerLoader resuming on restart!");
            }
            catch (Exception ex)
            {
                TweaksLog.Error("PlayerLoaderResumingPatch error", ex);
            }
            finally
            {
                InstantRestartPatch.IsRestarting = false;
            }
        }
    }
}

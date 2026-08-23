using System;
using osu.Framework.Bindables;
using osu.Game.Screens.Play;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч для мгновенного перезапуска карты БЕЗ задержек только при явном рестарте (Ctrl+R / Retry),
    /// не ломая плавную анимацию загрузчика при обычном входе из меню выбора карт.
    /// </summary>
    public sealed class InstantRestartPatch : PluginPatch<OsuTweaksPlugin>
    {
        private static Bindable<bool>? instantRetryBindable;
        private static bool isExplicitRestart;

        public InstantRestartPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<bool> instantRetry)
            : base(plugin, host, "osu.Game.Screens.Play.Player", "Restart", MethodType.Prefix)
        {
            instantRetryBindable = instantRetry;
        }

        public static void Prefix()
        {
            isExplicitRestart = true;
            TweaksLog.Info("InstantRestartPatch: Player.Restart called - flagged isExplicitRestart = true");
        }

        public static void OnPlayerLoaderEntering(PlayerLoader loader)
        {
            if (loader == null) return;

            if (isExplicitRestart && instantRetryBindable?.Value == true)
            {
                try
                {
                    loader.FinishTransforms(true);
                    TweaksLog.Info("InstantRestartPatch: Fast-forwarded PlayerLoader entrance for zero-delay restart!");
                }
                catch (Exception ex)
                {
                    TweaksLog.Error("InstantRestartPatch: Error finishing transforms on restart", ex);
                }
            }

            // Сбрасываем флаг для последующих нормальных входов из меню
            isExplicitRestart = false;
        }
    }

    /// <summary>
    /// Вспомогательный патч на PlayerLoader.OnEntering для применения мгновенного входа только при рестарте.
    /// </summary>
    public sealed class PlayerLoaderRestartPatch : PluginPatch<OsuTweaksPlugin>
    {
        public PlayerLoaderRestartPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Screens.Play.PlayerLoader", "OnEntering", MethodType.Postfix)
        {
        }

        public static void Postfix(PlayerLoader __instance)
        {
            InstantRestartPatch.OnPlayerLoaderEntering(__instance);
        }
    }
}

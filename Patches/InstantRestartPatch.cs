using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Play;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на Player.Restart для фиксации намерения мгновенного перезапуска.
    /// </summary>
    public sealed class InstantRestartPatch : PluginPatch<OsuTweaksPlugin>
    {
        public static Bindable<bool>? InstantRetryBindable { get; private set; }
        public static bool IsRestarting { get; set; }

        public InstantRestartPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<bool> instantRetry)
            : base(plugin, host, "osu.Game.Screens.Play.Player", "Restart", MethodType.Prefix)
        {
            InstantRetryBindable = instantRetry;
        }

        public static void Prefix()
        {
            if (InstantRetryBindable?.Value == true)
            {
                IsRestarting = true;
                TweaksLog.Info("InstantRestartPatch: Player.Restart triggered - fast restart enabled!");
            }
        }
    }

    /// <summary>
    /// Патч на Player.OnExiting для мгновенного завершения анимации выхода старого игрока при рестарте.
    /// </summary>
    public sealed class PlayerExitingRestartPatch : PluginPatch<OsuTweaksPlugin>
    {
        public PlayerExitingRestartPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Screens.Play.Player", "OnExiting", MethodType.Prefix)
        {
        }

        public static void Prefix(Player __instance)
        {
            if (__instance == null || !InstantRestartPatch.IsRestarting || InstantRestartPatch.InstantRetryBindable?.Value != true)
                return;

            try
            {
                // Мгновенно скрываем и завершаем трансформации старого экрана игры
                __instance.FinishTransforms(true);
                __instance.Alpha = 0f;
            }
            catch (Exception ex)
            {
                TweaksLog.Error("PlayerExitingRestartPatch error", ex);
            }
        }
    }

    /// <summary>
    /// Патч на PlayerLoader.OnResuming для сброса задержек загрузчика при возобновлении после рестарта.
    /// </summary>
    public sealed class PlayerLoaderResumingPatch : PluginPatch<OsuTweaksPlugin>
    {
        public PlayerLoaderResumingPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Screens.Play.PlayerLoader", "OnResuming", MethodType.Postfix)
        {
        }

        public static void Postfix(PlayerLoader __instance)
        {
            if (__instance == null || InstantRestartPatch.InstantRetryBindable?.Value != true)
                return;

            try
            {
                // Завершаем трансформации экрана загрузки при рестарте
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

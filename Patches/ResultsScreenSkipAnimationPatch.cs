using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Ranking;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на ResultsScreen.OnEntering для мгновенного завершения анимаций экрана результатов
    /// (экспанд панели, вращение круга точности и набегание счетчиков).
    /// </summary>
    public sealed class ResultsScreenSkipAnimationPatch : PluginPatch<OsuTweaksPlugin>
    {
        private static Bindable<bool>? skipResultsBindable;
        private static IOsuCcPluginHost? pluginHost;

        public ResultsScreenSkipAnimationPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<bool> skipResults)
            : base(plugin, host, "osu.Game.Screens.Ranking.ResultsScreen", "OnEntering", MethodType.Postfix)
        {
            skipResultsBindable = skipResults;
            pluginHost = host;
        }

        public static void Postfix(ResultsScreen __instance)
        {
            if (__instance == null || skipResultsBindable?.Value != true)
                return;

            try
            {
                // 1. Моментально завершаем анимации самого экрана ResultsScreen
                __instance.FinishTransforms(true);

                // 2. Откладываем завершение трансформаций дочерних панелей для перехвата отложенных анимаций
                pluginHost?.Scheduler?.AddOnce(() => finishSubtreeTransforms(__instance));
                pluginHost?.Scheduler?.AddDelayed(() => finishSubtreeTransforms(__instance), 50);

                TweaksLog.Info("ResultsScreenSkipAnimationPatch: Fast-forwarded results screen animations!");
            }
            catch (Exception ex)
            {
                TweaksLog.Error("ResultsScreenSkipAnimationPatch error", ex);
            }
        }

        private static void finishSubtreeTransforms(ResultsScreen resultsScreen)
        {
            try
            {
                resultsScreen.FinishTransforms(true);

                foreach (var panel in resultsScreen.ChildrenOfType<ScorePanel>())
                {
                    panel.FinishTransforms(true);
                }

                foreach (var drawable in resultsScreen.ChildrenOfType<Drawable>())
                {
                    string name = drawable.GetType().Name;
                    if (name.Contains("Counter", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Accuracy", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("RankBadge", StringComparison.OrdinalIgnoreCase))
                    {
                        drawable.FinishTransforms(true);
                    }
                }
            }
            catch
            {
                // ignore disposed exceptions
            }
        }
    }
}

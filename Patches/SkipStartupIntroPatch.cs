using System;
using System.Reflection;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osucc.Core;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на IntroScreen.OnEntering для мгновенного пропуска стартовых заставок (Circles, Triangles, Welcome) прямо в Главное меню.
    /// </summary>
    public sealed class SkipStartupIntroPatch : PluginPatch<OsuTweaksPlugin>
    {
        private static Bindable<bool>? skipIntroBindable;

        public SkipStartupIntroPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host, Bindable<bool> skipIntro)
            : base(plugin, host, "osu.Game.Screens.Menu.IntroScreen", "OnEntering", MethodType.Postfix)
        {
            skipIntroBindable = skipIntro;
        }

        public static void Postfix(IntroScreen __instance)
        {
            if (__instance == null || skipIntroBindable?.Value != true)
                return;

            try
            {
                // Завершаем трансформации экрана интро
                __instance.FinishTransforms(true);

                // Если главное меню еще не было запущено, принудительно пушим MainMenu
                if (!__instance.DidLoadMenu)
                {
                    var createNextScreenField = ReflectionHelper.FindField(__instance.GetType(), "createNextScreen");
                    Func<OsuScreen>? createNextScreen = null;

                    if (createNextScreenField != null)
                    {
                        createNextScreen = createNextScreenField.GetValue(__instance) as Func<OsuScreen>;
                    }

                    var nextScreen = createNextScreen?.Invoke() ?? new MainMenu();
                    __instance.Push(nextScreen);

                    TweaksLog.Info("SkipStartupIntroPatch: Instantly skipped startup intro to MainMenu!");
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("SkipStartupIntroPatch error", ex);
            }
        }
    }
}

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK.Graphics;
using osucc.Plugin;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Запасной монитор для нейтрализации GameWideFlash или других белых полноэкранных вспышек на старте.
    /// </summary>
    public class IntroFlashCustomizer : IDisposable
    {
        private readonly IOsuCcPluginHost host;
        private readonly Bindable<bool> darkIntroFlash = new(true);
        private ScheduledDelegate? monitorSchedule;
        private bool isHooked;

        public IntroFlashCustomizer(IOsuCcPluginHost host)
        {
            this.host = host;
        }

        public void Attach(Bindable<bool> darkFlashBindable)
        {
            darkIntroFlash.UnbindBindings();
            darkIntroFlash.BindTo(darkFlashBindable);

            if (isHooked) return;
            isHooked = true;

            monitorSchedule = host.Scheduler?.AddDelayed(checkIntroFlashes, 20, true);
        }

        private void checkIntroFlashes()
        {
            if (host.Game == null || !darkIntroFlash.Value) return;

            try
            {
                if (host.Game is Drawable gameRoot)
                {
                    foreach (var box in gameRoot.ChildrenOfType<Box>())
                    {
                        string typeName = box.GetType().Name;
                        if (typeName.Contains("GameWideFlash", StringComparison.OrdinalIgnoreCase) ||
                            (box.Blending == BlendingParameters.Additive && box.Colour == Color4.White && box.RelativeSizeAxes == Axes.Both))
                        {
                            box.Colour = Color4.Black;
                            box.Blending = BlendingParameters.Inherit;
                            box.Alpha = 0f;
                            box.Expire();
                            TweaksLog.Info("IntroFlashCustomizer: Intercepted and silenced white flash box!");
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем исключения при disposed
            }
        }

        public void Dispose()
        {
            monitorSchedule?.Cancel();
            monitorSchedule = null;
            isHooked = false;
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osuTK.Graphics;
using osucc.Plugin;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Заменяет ослепляющую белую вспышку (GameWideFlash) при запуске игры на мягкое тёмное затухание.
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

            monitorSchedule = host.Scheduler?.AddDelayed(checkIntroFlashes, 50, true);
        }

        private void checkIntroFlashes()
        {
            if (host.Game == null || !darkIntroFlash.Value) return;

            try
            {
                // Ищем GameWideFlash, добавляемый в корень игры при старте
                foreach (var child in host.Game.Children.OfType<Drawable>().ToList())
                {
                    string typeName = child.GetType().Name;
                    if (typeName.Contains("GameWideFlash", StringComparison.OrdinalIgnoreCase))
                    {
                        if (child is Box flashBox)
                        {
                            flashBox.Colour = Color4.Black;
                            flashBox.Blending = BlendingParameters.Inherit;
                            flashBox.Alpha = 0f;
                            flashBox.Expire();
                            TweaksLog.Info("IntroFlashCustomizer: Intercepted and silenced white GameWideFlash on startup!");
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

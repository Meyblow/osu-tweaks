using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game;
using osu.Game.Overlays;
using osu.Game.Screens.Select;
using osucc.Plugin;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Ограничивает максимальную громкость предпрослушивания треков в меню выбора карт (Song Select).
    /// </summary>
    public class TweaksAudioLimiter : IDisposable
    {
        private readonly IOsuCcPluginHost host;
        private readonly Bindable<double> volumeLimit = new(0.6);
        private ScheduledDelegate? monitorSchedule;
        private bool isHooked;

        public TweaksAudioLimiter(IOsuCcPluginHost host)
        {
            this.host = host;
        }

        public void Attach(Bindable<double> limitBindable)
        {
            volumeLimit.UnbindBindings();
            volumeLimit.BindTo(limitBindable);

            if (isHooked) return;
            isHooked = true;

            monitorSchedule = host.Scheduler?.AddDelayed(checkAndLimitVolume, 100, true);
        }

        private void checkAndLimitVolume()
        {
            if (host.Game is not OsuGame game) return;

            try
            {
                // Проверяем, находимся ли мы на экране выбора карт
                if (game.ScreenStack?.CurrentScreen is SongSelect)
                {
                    var musicController = ReflectionHelper.GetPropertyValue<MusicController>(game, "MusicController")
                                          ?? game.ChildrenOfType<MusicController>().FirstOrDefault();

                    var track = musicController?.CurrentTrack;
                    if (track != null && track.IsRunning)
                    {
                        double maxVol = Math.Clamp(volumeLimit.Value, 0.05, 1.0);
                        if (track.Volume.Value > maxVol)
                        {
                            track.Volume.Value = maxVol;
                        }
                    }
                }
            }
            catch
            {
                // ignore exceptions
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

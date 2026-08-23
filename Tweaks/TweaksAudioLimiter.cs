using System;
using System.Linq;
using osu.Framework.Audio.Track;
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
    /// Полностью соблюдает принцип обратимости (Reversibility): сохраняет и восстанавливает исходную громкость треков.
    /// </summary>
    public class TweaksAudioLimiter : IDisposable
    {
        private readonly IOsuCcPluginHost host;
        private readonly Bindable<double> volumeLimit = new(0.6);
        private ScheduledDelegate? monitorSchedule;
        private bool isHooked;

        private ITrack? trackedTrack;
        private double originalVolume = 1.0;
        private bool isVolumeModified;

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
                        // Если трек сменился, сбрасываем предыдущий и запоминаем оригинальную громкость нового
                        if (track != trackedTrack)
                        {
                            restoreTrackedVolume();
                            trackedTrack = track;
                            originalVolume = track.Volume.Value;
                            isVolumeModified = false;
                        }

                        double maxVol = Math.Clamp(volumeLimit.Value, 0.05, 1.0);

                        // Если громкость превышает лимит
                        if (originalVolume > maxVol)
                        {
                            track.Volume.Value = maxVol;
                            isVolumeModified = true;
                        }
                        else if (isVolumeModified)
                        {
                            // Если лимит повысили выше оригинала
                            track.Volume.Value = originalVolume;
                            isVolumeModified = false;
                        }
                    }
                }
                else
                {
                    // При уходе из SongSelect возвращаем громкость
                    restoreTrackedVolume();
                }
            }
            catch
            {
                // ignore exceptions
            }
        }

        private void restoreTrackedVolume()
        {
            if (trackedTrack != null && isVolumeModified)
            {
                try
                {
                    trackedTrack.Volume.Value = originalVolume;
                }
                catch { }

                isVolumeModified = false;
            }
        }

        public void Dispose()
        {
            monitorSchedule?.Cancel();
            monitorSchedule = null;

            restoreTrackedVolume();
            trackedTrack = null;

            isHooked = false;
            GC.SuppressFinalize(this);
        }
    }
}

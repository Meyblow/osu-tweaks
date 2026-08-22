using System;
using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Toolbar;
using osucc.Plugin;
using OsuTweaks.Models;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Управляет форматированием времени, даты и таймера сессии в ToolbarClock.
    /// </summary>
    public class ToolbarClockCustomizer : IDisposable
    {
        private static readonly DateTime sessionStartTime = DateTime.Now;

        private readonly IOsuCcPluginHost host;
        private ToolbarClock? toolbarClock;
        private DigitalClockDisplay? digitalDisplay;
        private OsuSpriteText? realTimeText;
        private OsuSpriteText? gameTimeText;
        private FillFlowContainer? runningTextFlow;
        private ScheduledDelegate? updateSchedule;

        private readonly Bindable<ClockDisplayFormat> formatBindable = new(ClockDisplayFormat.StandardWithSeconds);
        private readonly Bindable<bool> showSessionTimerBindable = new(false);

        public ToolbarClockCustomizer(IOsuCcPluginHost host)
        {
            this.host = host;
        }

        public void Attach(ToolbarClock clock, Bindable<ClockDisplayFormat> format, Bindable<bool> showSessionTimer)
        {
            toolbarClock = clock;
            formatBindable.UnbindBindings();
            formatBindable.BindTo(format);

            showSessionTimerBindable.UnbindBindings();
            showSessionTimerBindable.BindTo(showSessionTimer);

            findClockComponents();

            updateSchedule?.Cancel();
            updateSchedule = host.Scheduler?.AddDelayed(updateClock, 250, true);
        }

        private void findClockComponents()
        {
            if (toolbarClock == null) return;

            try
            {
                digitalDisplay = toolbarClock.ChildrenOfType<DigitalClockDisplay>().FirstOrDefault()
                                 ?? ReflectionHelper.GetFieldValue<DigitalClockDisplay>(toolbarClock, "digital");

                if (digitalDisplay != null)
                {
                    realTimeText = ReflectionHelper.GetFieldValue<OsuSpriteText>(digitalDisplay, "realTime");
                    gameTimeText = ReflectionHelper.GetFieldValue<OsuSpriteText>(digitalDisplay, "gameTime");
                    runningTextFlow = ReflectionHelper.GetFieldValue<FillFlowContainer>(digitalDisplay, "runningText");
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("ToolbarClockCustomizer: Error locating clock components", ex);
            }
        }

        private void updateClock()
        {
            if (toolbarClock == null || !toolbarClock.IsAlive) return;

            if (digitalDisplay == null || realTimeText == null)
            {
                findClockComponents();
                if (digitalDisplay == null || realTimeText == null) return;
            }

            try
            {
                var now = DateTime.Now;
                var culture = CultureInfo.CurrentCulture;

                switch (formatBindable.Value)
                {
                    case ClockDisplayFormat.CompactNoSeconds:
                        realTimeText.Text = now.ToString("HH:mm", culture);
                        break;

                    case ClockDisplayFormat.WithDate:
                        realTimeText.Text = now.ToString("dd MMM · HH:mm", culture);
                        break;

                    case ClockDisplayFormat.WithDateAndSeconds:
                        realTimeText.Text = now.ToString("dd MMM · HH:mm:ss", culture);
                        break;

                    case ClockDisplayFormat.SessionTimerOnly:
                        var elapsedOnly = now - sessionStartTime;
                        realTimeText.Text = $"⏳ {(int)elapsedOnly.TotalHours}ч {elapsedOnly.Minutes:D2}м";
                        break;

                    default: // StandardWithSeconds
                        realTimeText.Text = now.ToString("HH:mm:ss", culture);
                        break;
                }

                if (runningTextFlow != null && gameTimeText != null)
                {
                    if (showSessionTimerBindable.Value && formatBindable.Value != ClockDisplayFormat.SessionTimerOnly)
                    {
                        var elapsed = now - sessionStartTime;
                        gameTimeText.Text = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                        runningTextFlow.Alpha = 1;
                    }
                    else if (!showSessionTimerBindable.Value)
                    {
                        runningTextFlow.Alpha = 0;
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
            updateSchedule?.Cancel();
            updateSchedule = null;
            toolbarClock = null;
            digitalDisplay = null;
            realTimeText = null;
            gameTimeText = null;
            runningTextFlow = null;
            GC.SuppressFinalize(this);
        }
    }
}

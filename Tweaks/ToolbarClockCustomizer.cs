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
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Toolbar;
using osuTK;
using osucc.Plugin;
using OsuTweaks.Models;
using OsuTweaks.Utils;

namespace OsuTweaks.Tweaks
{
    /// <summary>
    /// Управляет форматированием времени, даты и таймера сессии в ToolbarClock.
    /// Заменяет стандартный вывод на отдельный стабильный контейнер, предотвращая мерцание и сброс ванильного Text.
    /// </summary>
    public class ToolbarClockCustomizer : IDisposable
    {
        private static readonly DateTime sessionStartTime = DateTime.Now;

        private readonly IOsuCcPluginHost host;
        private ToolbarClock? toolbarClock;
        private FillFlowContainer? clockFlow;
        private DigitalClockDisplay? digitalDisplay;
        private AnalogClockDisplay? analogDisplay;

        private FillFlowContainer? customClockContainer;
        private OsuSpriteText? customRealTime;
        private FillFlowContainer? customSessionFlow;
        private OsuSpriteText? customSessionText;

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
            formatBindable.BindValueChanged(_ => updateClock(), false);

            showSessionTimerBindable.UnbindBindings();
            showSessionTimerBindable.BindTo(showSessionTimer);
            showSessionTimerBindable.BindValueChanged(_ => updateClock(), false);

            setupCustomClock();

            updateSchedule?.Cancel();
            updateSchedule = host.Scheduler?.AddDelayed(updateClock, 200, true);
        }

        private void setupCustomClock()
        {
            if (toolbarClock == null) return;

            try
            {
                digitalDisplay = toolbarClock.ChildrenOfType<DigitalClockDisplay>().FirstOrDefault()
                                 ?? ReflectionHelper.GetFieldValue<DigitalClockDisplay>(toolbarClock, "digital");

                analogDisplay = toolbarClock.ChildrenOfType<AnalogClockDisplay>().FirstOrDefault()
                                ?? ReflectionHelper.GetFieldValue<AnalogClockDisplay>(toolbarClock, "analog");

                clockFlow = toolbarClock.ChildrenOfType<FillFlowContainer>().FirstOrDefault();

                if (analogDisplay != null)
                {
                    analogDisplay.Alpha = 0;
                }

                if (digitalDisplay != null)
                {
                    digitalDisplay.Alpha = 0;
                }

                if (clockFlow != null && customClockContainer == null)
                {
                    customClockContainer = new FillFlowContainer
                    {
                        Name = "osu!tweaks Custom Clock Display",
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        AlwaysPresent = true,
                        Children = new Drawable[]
                        {
                            customRealTime = new OsuSpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Font = OsuFont.Default.With(fixedWidth: true),
                                Spacing = new Vector2(-1.5f, 0)
                            },
                            customSessionFlow = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(2f, 0),
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Colour = Colour4.FromHex("#ff66aa"),
                                Alpha = 0,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Text = "SESSION",
                                        Font = OsuFont.Default.With(size: 10f, weight: FontWeight.SemiBold)
                                    },
                                    (customSessionText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Font = OsuFont.Default.With(size: 10f, weight: FontWeight.SemiBold, fixedWidth: true),
                                        Spacing = new Vector2(-0.5f, 0)
                                    })
                                }
                            }
                        }
                    };

                    clockFlow.Add(customClockContainer);
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("ToolbarClockCustomizer: Error setting up custom clock", ex);
            }
        }

        private void updateClock()
        {
            if (toolbarClock == null || !toolbarClock.IsAlive) return;

            if (customClockContainer == null || customRealTime == null)
            {
                setupCustomClock();
                if (customClockContainer == null || customRealTime == null) return;
            }

            try
            {
                if (digitalDisplay != null) digitalDisplay.Alpha = 0;
                if (analogDisplay != null) analogDisplay.Alpha = 0;

                var now = DateTime.Now;
                var culture = CultureInfo.CurrentCulture;

                switch (formatBindable.Value)
                {
                    case ClockDisplayFormat.CompactNoSeconds:
                        customRealTime.Text = now.ToString("HH:mm", culture);
                        break;

                    case ClockDisplayFormat.WithDate:
                        customRealTime.Text = now.ToString("dd MMM · HH:mm", culture);
                        break;

                    case ClockDisplayFormat.WithDateAndSeconds:
                        customRealTime.Text = now.ToString("dd MMM · HH:mm:ss", culture);
                        break;

                    case ClockDisplayFormat.SessionTimerOnly:
                        var elapsedOnly = now - sessionStartTime;
                        customRealTime.Text = $"⏳ {(int)elapsedOnly.TotalHours}h {elapsedOnly.Minutes:D2}m";
                        break;

                    default: // StandardWithSeconds
                        customRealTime.Text = now.ToString("HH:mm:ss", culture);
                        break;
                }

                if (customSessionFlow != null && customSessionText != null)
                {
                    if (showSessionTimerBindable.Value && formatBindable.Value != ClockDisplayFormat.SessionTimerOnly)
                    {
                        var elapsed = now - sessionStartTime;
                        customSessionText.Text = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                        customSessionFlow.Alpha = 1;
                    }
                    else
                    {
                        customSessionFlow.Alpha = 0;
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

            if (customClockContainer != null && clockFlow != null)
            {
                try { clockFlow.Remove(customClockContainer, true); } catch { }
            }

            if (digitalDisplay != null)
            {
                digitalDisplay.Alpha = 1;
            }

            toolbarClock = null;
            clockFlow = null;
            digitalDisplay = null;
            analogDisplay = null;
            customClockContainer = null;
            customRealTime = null;
            customSessionFlow = null;
            customSessionText = null;
            GC.SuppressFinalize(this);
        }
    }
}

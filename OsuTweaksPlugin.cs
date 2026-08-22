using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game;
using osu.Game.Overlays.Toolbar;
using osucc.Plugin;
using OsuTweaks.Patches;
using OsuTweaks.Tweaks;
using OsuTweaks.UI;

namespace OsuTweaks
{
    /// <summary>
    /// Главная точка входа плагина osu!tweaks для osu!cc 3.0.0.
    /// </summary>
    public class OsuTweaksPlugin : OsuCcPlugin
    {
        public static OsuTweaksPlugin? Instance { get; private set; }

        public Bindable<bool> AutoSkipBreaks { get; private set; } = new(false);

        private ModularToolbarManager? toolbarManager;

        protected override void OnLoad()
        {
            Instance = this;
            TweaksLog.Init(Host);
            TweaksLog.Info("osu!tweaks: OnLoad() starting...");

            AutoSkipBreaks = Host.GetSettings().Bind("auto_skip_breaks", false);

            toolbarManager = new ModularToolbarManager(Host);
            Host.AddPatch(new ToolbarPatch(this, Host));
            Host.AddPatch(new PlayerBreakAutoSkipPatch(this, Host));

            TweaksLog.Info("osu!tweaks: OnLoad() complete.");
        }

        public override void AttachToGame()
        {
            TweaksLog.Info("osu!tweaks: AttachToGame() called.");

            Host.AddSettingsSubsection(() => new TweaksSettingsSubsection(Host.GetSettings()));

            if (Host.Game is OsuGame game)
            {
                Host.Scheduler?.Add(() =>
                {
                    try
                    {
                        var toolbar = game.ChildrenOfType<Toolbar>().FirstOrDefault();
                        if (toolbar != null)
                        {
                            TweaksLog.Info($"AttachToGame: Found already existing Toolbar ({toolbar.GetHashCode()}), attaching manager.");
                            toolbarManager?.AttachToolbar(toolbar);
                        }
                    }
                    catch (Exception ex)
                    {
                        TweaksLog.Error("Error checking Toolbar in AttachToGame scheduler", ex);
                    }
                });
            }
        }

        internal void OnToolbarLoaded(Toolbar toolbar)
        {
            TweaksLog.Info($"OnToolbarLoaded received Toolbar ({toolbar.GetHashCode()})");
            toolbarManager?.AttachToolbar(toolbar);
        }

        public override void Dispose()
        {
            TweaksLog.Info("osu!tweaks: Disposing plugin...");
            toolbarManager?.Dispose();
            toolbarManager = null;

            base.Dispose();
            GC.SuppressFinalize(this);
            TweaksLog.Info("osu!tweaks: Plugin disposed.");
        }
    }
}

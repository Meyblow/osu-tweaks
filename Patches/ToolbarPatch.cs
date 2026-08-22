using osu.Game.Overlays.Toolbar;
using osucc.Plugin;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч жизненного цикла тулбара osu!lazer через официальный PluginPatch API в 3.0.0.
    /// </summary>
    public sealed class ToolbarPatch : PluginPatch<OsuTweaksPlugin>
    {
        public ToolbarPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Overlays.Toolbar.Toolbar", "load")
        {
        }

        public static void Postfix(Toolbar __instance)
        {
            TweaksLog.Info($"Toolbar.load Postfix triggered! Toolbar instance: {__instance?.GetHashCode()}");
            if (__instance != null)
            {
                OsuTweaksPlugin.Instance?.OnToolbarLoaded(__instance);
            }
        }
    }
}

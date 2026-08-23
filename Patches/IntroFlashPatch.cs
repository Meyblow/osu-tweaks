using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using osucc.Core;
using osucc.Plugin;

namespace OsuTweaks.Patches
{
    /// <summary>
    /// Патч на GameWideFlash.LoadComplete в IntroTriangles для мгновенного гашения белой ослепляющей вспышки.
    /// </summary>
    public sealed class IntroFlashPatch : PluginPatch<OsuTweaksPlugin>
    {
        public IntroFlashPatch(OsuTweaksPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Screens.Menu.IntroTriangles+TrianglesIntroSequence+GameWideFlash", "LoadComplete", MethodType.Prefix)
        {
        }

        public static bool Prefix(Box __instance)
        {
            if (__instance == null) return true;

            try
            {
                var plugin = OsuTweaksPlugin.Instance;
                if (plugin != null && plugin.DarkIntroFlash.Value)
                {
                    __instance.Colour = Color4.Black;
                    __instance.Blending = BlendingParameters.Inherit;
                    __instance.Alpha = 0f;
                    __instance.Expire();
                    TweaksLog.Info("IntroFlashPatch: Successfully neutralized GameWideFlash!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                TweaksLog.Error("IntroFlashPatch exception in Prefix", ex);
            }

            return true;
        }
    }
}

using BepInEx;
using HarmonyLib;
using ModdingTales;
using BepInEx.Logging;
using PluginUtilities;
using BepInEx.Configuration;

namespace AutoInitiative
{
    [BepInPlugin(Guid, "Auto Initiative Plugin", Version)]
    [BepInDependency(SetInjectionFlag.Guid)]
    public class AutoInitiativePlugin : BaseUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.AutoInitiative";
        internal const string Version = "1.1.0.0";

        // Config
        internal static ConfigEntry<string> InitiativeText;

        internal static ManualLogSource logger;

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            logger = Logger;

            Logger.LogDebug("Auto Initiative loaded");

            InitiativeText = Config.Bind("Initiative", "Required Text", "Initiative");

            try {
                var harmony = new Harmony(Guid);
                harmony.PatchAll();
                ModdingUtils.AddPluginToMenuList(this, "HolloFoxes'");
            }
            catch (System.Exception e)
            {
                logger.LogError(e.Message);
            }
        }
    }
}

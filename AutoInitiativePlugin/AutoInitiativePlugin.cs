using BepInEx;
using HarmonyLib;
using ModdingTales;
using BepInEx.Logging;
using PluginUtilities;

namespace AutoInitiative
{
    [BepInPlugin(Guid, "Auto Initiative Plugin", Version)]
    [BepInDependency(SetInjectionFlag.Guid)]
    public class AutoInitiativePlugin : BaseUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.AutoInitiative";
        internal const string Version = "1.0.0.0";

        internal static ManualLogSource logger;

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            logger = Logger;

            Logger.LogDebug("Auto Initiative loaded");
            
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

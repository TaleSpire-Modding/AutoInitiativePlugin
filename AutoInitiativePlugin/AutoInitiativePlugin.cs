using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using PluginUtilities;
using BepInEx.Configuration;

namespace AutoInitiative
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(SetInjectionFlag.Guid)]
    public class AutoInitiativePlugin : DependencyUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.AutoInitiative";
        internal const string Version = "0.0.0.0";
        public const string Name = "Auto Initiative Plugin";

        // Config
        internal static ConfigEntry<string> InitiativeText;

        internal static ManualLogSource logger;
        Harmony harmony;

        /// <summary>
        /// Awake plugin
        /// </summary>
        protected override void OnAwake()
        {
            logger = Logger;

            Logger.LogDebug("Auto Initiative loaded");

            InitiativeText = Config.Bind("Initiative", "Required Text", "Initiative");

            try {
                harmony = new Harmony(Guid);
                harmony.PatchAll();
            }
            catch (System.Exception e)
            {
                logger.LogError(e.Message);
            }
        }

        protected override void OnDestroyed()
        {
            harmony.UnpatchSelf();
            Patches.InitUtils.ClearInitiatives();
        }
    }
}

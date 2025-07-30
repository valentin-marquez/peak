using BepInEx;
using BepInEx.Logging;
using ClimbTunes.Core;

namespace ClimbTunes
{
    [BepInPlugin("com.nozz.climbtunes", "ClimbTunes", "1.0.0")]
    public class ClimbTunes : BaseUnityPlugin
    {
        private ModInitializer modInitializer;
        public static ManualLogSource ModLogger { get; private set; }

        private void Awake()
        {
            ModLogger = Logger;
            ModLogger.LogInfo("ClimbTunes mod is starting...");

            modInitializer = new ModInitializer(this);
            modInitializer.Initialize();
            
            ModLogger.LogInfo("ClimbTunes mod loaded successfully!");
        }

        private void OnDestroy()
        {
            modInitializer?.Cleanup();
        }
    }
}
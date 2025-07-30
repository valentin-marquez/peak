using BepInEx;
using BepInEx.Logging;
using HDPeak.Settings;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zorro.Settings;
using SettingsExtender;
using HarmonyLib;

namespace HDPeak
{
    /// <summary>
    /// Plugin information for HDPeak mod
    /// </summary>
    internal static class HDPeakPluginInfo
    {
        internal const string PLUGIN_GUID = "com.nozz.hdpeak";
        internal const string PLUGIN_NAME = "HDPeak";
        internal const string PLUGIN_VERSION = "1.0.0";
    }

    /// <summary>
    /// Main plugin class for HDPeak - Advanced graphics settings mod for PEAK
    /// </summary>
    [BepInPlugin(HDPeakPluginInfo.PLUGIN_GUID, HDPeakPluginInfo.PLUGIN_NAME, HDPeakPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.pharmacomaniac.settingsextenderforked")]
    public class HDPeakPlugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        internal static Harmony harmony;
        private GameObject buttonSrc;
        private bool settingsAdded = false;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {HDPeakPluginInfo.PLUGIN_GUID} is loaded!");

            // Register custom settings page
            SettingsRegistry.Register("HDPeak");

            // Initialize Harmony for patching
            harmony = new Harmony(HDPeakPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void Start()
        {
            // Add all graphics settings once during startup
            if (!settingsAdded)
            {
                try
                {
                    Logger.LogInfo("Adding Anti-Aliasing setting...");
                    SettingsHandler.Instance.AddSetting(new AntiAliasingSetting());
                    Logger.LogInfo("Adding Anisotropic Filtering setting...");
                    SettingsHandler.Instance.AddSetting(new AnisotropicFilteringSetting());
                    Logger.LogInfo("Adding Texture Quality setting...");
                    SettingsHandler.Instance.AddSetting(new TextureQualitySetting());
                    Logger.LogInfo("Adding Shadow Resolution setting...");
                    SettingsHandler.Instance.AddSetting(new ShadowResolutionSetting());
                    Logger.LogInfo("Adding LOD Bias setting...");
                    SettingsHandler.Instance.AddSetting(new LODBiasSetting());
                    Logger.LogInfo("Adding Opaque Texture setting...");
                    SettingsHandler.Instance.AddSetting(new OpaqueTextureSetting());
                    Logger.LogInfo("Adding Max Additional Lights setting...");
                    SettingsHandler.Instance.AddSetting(new MaxLightsSetting());
                    Logger.LogInfo("Adding Dynamic Batching setting...");
                    SettingsHandler.Instance.AddSetting(new DynamicBatchingSetting());
                    Logger.LogInfo("Adding Culling Distance setting...");
                    SettingsHandler.Instance.AddSetting(new CullingDistanceSetting());

                    settingsAdded = true;
                    Logger.LogInfo("HDPeak settings added successfully!");
                    Logger.LogInfo("Available settings: Anti-Aliasing, Anisotropic Filtering, Texture Quality, Shadow Resolution, LOD Bias, Opaque Texture, Max Additional Lights, Dynamic Batching, Culling Distance");
                    Logger.LogInfo($"Settings added: {settingsAdded}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error adding settings: {ex.Message}");
                }
            }
        }
    }
}

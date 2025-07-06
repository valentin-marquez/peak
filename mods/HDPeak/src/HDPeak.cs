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
    public class HDPeakPlugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        private GameObject buttonSrc;
        private bool settingsAdded = false;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {HDPeakPluginInfo.PLUGIN_GUID} is loaded!");

            // Register custom settings page
            HDPeakSettingsRegistry.Register("HDPeak");
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
                    settingsAdded = true;
                    Logger.LogInfo("HDPeak settings added successfully!");
                    Logger.LogInfo("Available settings: Anti-Aliasing, Anisotropic Filtering, Texture Quality, Shadow Resolution, LOD Bias, Opaque Texture, Max Additional Lights, Dynamic Batching");
                    Logger.LogInfo($"Settings added: {settingsAdded}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error adding settings: {ex.Message}");
                }
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (updateTabsRoutine != null)
            {
                StopCoroutine(updateTabsRoutine);
                updateTabsRoutine = null;
            }
        }

        private Coroutine updateTabsRoutine;

        /// <summary>
        /// Handles scene loading to update settings UI tabs
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (updateTabsRoutine != null)
            {
                StopCoroutine(updateTabsRoutine);
                updateTabsRoutine = null;
            }

            updateTabsRoutine = StartCoroutine(UpdateTabs(scene));
        }

        /// <summary>
        /// Updates settings tabs UI in the loaded scene
        /// </summary>
        private IEnumerator UpdateTabs(Scene scene)
        {
            Logger.LogInfo("Scene loaded: " + scene.name);

            // Wait for settings button to be available
            while (buttonSrc == null)
            {
                List<SettingsTABSButton> buttons = FindAllInScene(scene);
                if (buttons.Count != 0)
                {
                    buttonSrc = buttons[0].gameObject;
                }
                yield return new WaitForSeconds(.050f);
            }

            // Create custom settings tab
            if (buttonSrc != null)
            {
                Logger.LogInfo("Found TABS/General");

                foreach (var pair in HDPeakSettingsRegistry.GetPages())
                {
                    string name = pair.Key;
                    SettingsCategory category = pair.Value;
                    Logger.LogInfo($"Creating {name} with category {category}");
                    GameObject newButton = Instantiate(buttonSrc, buttonSrc.transform.parent);
                    SettingsTABSButton tabsButton = newButton.GetComponent<SettingsTABSButton>();
                    tabsButton.category = category;
                    tabsButton.text.text = name;
                }
            }
        }

        /// <summary>
        /// Finds all settings tab buttons in the current scene
        /// </summary>
        public static List<SettingsTABSButton> FindAllInScene(Scene scene)
        {
            var result = new List<SettingsTABSButton>();
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = root.GetComponentsInChildren<SettingsTABSButton>();
                result.AddRange(found);
            }
            return result;
        }
    }

    /// <summary>
    /// Registry for managing custom settings pages
    /// </summary>
    public class HDPeakSettingsRegistry
    {
        internal static Dictionary<string, SettingsCategory> nameToCategoryId = new();

        /// <summary>
        /// Registers a new settings page with a unique category ID
        /// </summary>
        public static void Register(string name)
        {
            if (!nameToCategoryId.ContainsKey(name))
            {
                SettingsCategory highestId = Enum.GetValues(typeof(SettingsCategory)).Cast<SettingsCategory>().Max();
                if (nameToCategoryId.Count != 0)
                {
                    highestId = nameToCategoryId.Values.Max();
                }

                nameToCategoryId[name] = highestId + 1;
            }
        }

        /// <summary>
        /// Gets the category ID for a registered page
        /// </summary>
        public static string GetPageId(string name)
        {
            return nameToCategoryId[name].ToString();
        }

        /// <summary>
        /// Gets all registered pages
        /// </summary>
        public static Dictionary<string, SettingsCategory> GetPages()
        {
            return nameToCategoryId;
        }
    }
}

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
using System.IO;
using System.Reflection;

namespace HDPeak
{
    internal static class HDPeakPluginInfo
    {
        internal const string PLUGIN_GUID = "com.nozz.hdpeak";
        internal const string PLUGIN_NAME = "HDPeak";
        internal const string PLUGIN_VERSION = "1.1.0";
    }

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
            LoadLocalizationData();
            HDPeakSettingsRegistry.Register("HDPeak");
        }

        private void Start()
        {
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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (updateTabsRoutine != null)
            {
                StopCoroutine(updateTabsRoutine);
                updateTabsRoutine = null;
            }
            updateTabsRoutine = StartCoroutine(UpdateTabs(scene));
        }

        private IEnumerator UpdateTabs(Scene scene)
        {
            Logger.LogInfo("Scene loaded: " + scene.name);
            while (buttonSrc == null)
            {
                List<SettingsTABSButton> buttons = FindAllInScene(scene);
                if (buttons.Count != 0)
                {
                    buttonSrc = buttons[0].gameObject;
                }
                yield return new WaitForSeconds(.050f);
            }
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
                    tabsButton.text.text = LocalizedText.GetText("HDPEAK_TAB");
                }
            }
        }

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

        private void LoadLocalizationData()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "HDPeak.assets.Resources.Localization.Localized_Text.csv";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Logger.LogError($"Could not find embedded resource: {resourceName}");
                        return;
                    }
                    using (var reader = new StreamReader(stream))
                    {
                        string csvContent = reader.ReadToEnd();
                        Logger.LogInfo("Successfully loaded HDPeak localization CSV");
                        ParseAndAddLocalizationData(csvContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading localization data: {ex.Message}");
            }
        }

        private void ParseAndAddLocalizationData(string csvContent)
        {
            try
            {
                var lines = csvContent.Split('\n');
                if (lines.Length < 2)
                {
                    Logger.LogError("Invalid CSV format: not enough lines");
                    return;
                }
                var header = lines[0].Split(',');
                var languageIndices = new Dictionary<string, int>();
                for (int i = 1; i < header.Length; i++)
                {
                    languageIndices[header[i].Trim()] = i;
                }
                Logger.LogInfo($"Found {languageIndices.Count} languages in localization file");
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    var columns = ParseCSVLine(line);
                    if (columns.Length < 2) continue;
                    string key = columns[0].Trim();
                    if (string.IsNullOrEmpty(key)) continue;
                    foreach (var lang in languageIndices)
                    {
                        if (lang.Value < columns.Length)
                        {
                            string value = columns[lang.Value].Trim();
                            if (!string.IsNullOrEmpty(value))
                            {
                                AddToGameLocalization(key, value, lang.Key);
                            }
                        }
                    }
                }
                Logger.LogInfo("Successfully parsed and added HDPeak localization data");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error parsing localization CSV: {ex.Message}");
            }
        }

        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string current = "";
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            result.Add(current);
            return result.ToArray();
        }

        private void AddToGameLocalization(string key, string value, string language)
        {
            try
            {
                var languageMap = new Dictionary<string, LocalizedText.Language>
                {
                    {"English", LocalizedText.Language.English},
                    {"French", LocalizedText.Language.French},
                    {"Italian", LocalizedText.Language.Italian},
                    {"German", LocalizedText.Language.German},
                    {"SpanishSpain", LocalizedText.Language.SpanishSpain},
                    {"SpanishLatam", LocalizedText.Language.SpanishLatam},
                    {"BRPortuguese", LocalizedText.Language.BRPortuguese},
                    {"Russian", LocalizedText.Language.Russian},
                    {"Ukrainian", LocalizedText.Language.Ukrainian},
                    {"SimplifiedChinese", LocalizedText.Language.SimplifiedChinese},
                    {"TraditionalChinese", LocalizedText.Language.TraditionalChinese},
                    {"Japanese", LocalizedText.Language.Japanese},
                    {"Korean", LocalizedText.Language.Korean}
                };
                if (!languageMap.ContainsKey(language))
                {
                    Logger.LogWarning($"Unknown language: {language}");
                    return;
                }
                var langEnum = languageMap[language];
                int langIndex = (int)langEnum;
                try
                {
                    var mainTableField = typeof(LocalizedText).GetField("MAIN_TABLE", 
                        BindingFlags.NonPublic | BindingFlags.Static);
                    
                    if (mainTableField != null)
                    {
                        var mainTable = mainTableField.GetValue(null) as Dictionary<string, List<string>>;
                        
                        if (mainTable != null)
                        {
                            if (!mainTable.ContainsKey(key))
                            {
                                var newList = new List<string>();
                                for (int i = 0; i < 13; i++)
                                {
                                    newList.Add("");
                                }
                                mainTable[key] = newList;
                            }
                            var list = mainTable[key];
                            if (langIndex < list.Count)
                            {
                                list[langIndex] = value;
                                Logger.LogDebug($"Added localization: {key} = {value} for {language}");
                            }
                            else
                            {
                                Logger.LogWarning($"Language index {langIndex} out of range for key {key}");
                            }
                        }
                        else
                        {
                            Logger.LogWarning("Main table is null, trying to initialize it");
                            LocalizedText.TryInitTables();
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Could not find MAIN_TABLE field in LocalizedText");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error accessing LocalizedText table: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error adding localization entry {key}: {ex.Message}");
            }
        }
    }

    // Registry for managing custom settings pages
    public class HDPeakSettingsRegistry
    {
        internal static Dictionary<string, SettingsCategory> nameToCategoryId = new();

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

        public static string GetPageId(string name)
        {
            return nameToCategoryId[name].ToString();
        }

        public static Dictionary<string, SettingsCategory> GetPages()
        {
            return nameToCategoryId;
        }
    }
}

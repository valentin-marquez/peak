using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Zorro.Core;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System;
using PEAKLib.Core;
using PEAKLib.Items;

namespace ClimbTunes
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("PEAKLib.Core")]
    [BepInDependency("PEAKLib.Items")]
    public class ClimbTunesPlugin : BaseUnityPlugin
    {
        public static ClimbTunesPlugin Instance { get; private set; }
        internal static new ManualLogSource Logger;

        // Configuration
        private ConfigEntry<bool> modEnabled;
        private ConfigEntry<float> radioVolume;
        private ConfigEntry<bool> autoSpawnRadio;
        private ConfigEntry<KeyCode> radioToggleKey;
        private ConfigEntry<string> defaultPlaylist;

        // Radio tracking
        private HashSet<int> playersWithRadios = new HashSet<int>();
        private bool gameStarted = false;
        private static readonly Dictionary<string, ClimbTunesRadio> activeRadios = new Dictionary<string, ClimbTunesRadio>();

        // UI References
        private GameObject radioUI;
        private bool isUIOpen = false;        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            
            // Configuration setup
            modEnabled = Config.Bind("General", "ModEnabled", true, "Enable/disable the mod");
            radioVolume = Config.Bind("Audio", "RadioVolume", 0.7f, "Default volume for radio playback (0.0 to 1.0)");
            autoSpawnRadio = Config.Bind("Gameplay", "AutoSpawnRadio", true, "Automatically spawn radio for all players at game start");
            radioToggleKey = Config.Bind("Controls", "RadioToggleKey", KeyCode.R, "Key to toggle radio UI");
            defaultPlaylist = Config.Bind("Audio", "DefaultPlaylist", "https://www.youtube.com/playlist?list=PLFgquLnL59alCl_2TQvOiD5Vgm1hCaGSI", "Default YouTube playlist URL");
            
            if (!modEnabled.Value)
            {
                Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is disabled!");
                return;
            }
            
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Logger.LogInfo("ClimbTunes - Portable radio system initialized");
            
            // Initialize radio item with PEAKLib
            ClimbTunesRadioItem.Initialize();
            
            Logger.LogInfo("PEAKLib integration completed");
            Logger.LogInfo("Photon networking support enabled");
            Logger.LogInfo("UI system support enabled");
            Logger.LogInfo("Audio system support enabled");
        }

        private void Start()
        {
            if (!modEnabled.Value) return;

            // Subscribe to scene change events
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Start the main monitoring coroutine
            StartCoroutine(MonitorGameState());
        }

        private void Update()
        {
            if (!modEnabled.Value) return;

            // Handle radio toggle key
            if (Input.GetKeyDown(radioToggleKey.Value))
            {
                ToggleRadioUI();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            gameStarted = false;
            playersWithRadios.Clear();
            Logger.LogInfo($"Scene changed to: {scene.name} - radio state reset");
        }

        private IEnumerator MonitorGameState()
        {
            while (true)
            {
                if (!modEnabled.Value)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Only activate in actual game levels
                if (!IsInValidGameLevel())
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Check if we're connected to Photon and game has started
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                {
                    if (!gameStarted && autoSpawnRadio.Value)
                    {
                        gameStarted = true;
                        Logger.LogInfo($"Game started in level: {SceneManager.GetActiveScene().name} - spawning radios");
                        yield return new WaitForSeconds(2f);
                        SpawnRadiosForAllPlayers();
                    }
                    else if (autoSpawnRadio.Value)
                    {
                        // Check for new players joining mid-game
                        CheckForNewPlayers();
                    }
                }
                else
                {
                    // Reset game state if disconnected
                    if (gameStarted)
                    {
                        gameStarted = false;
                        playersWithRadios.Clear();
                        Logger.LogInfo("Game session ended - resetting radio tracking");
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }

        private bool IsInValidGameLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            // Check if it's a Level_X scene (Level_0 through Level_13)
            if (sceneName.StartsWith("Level_"))
            {
                string levelNumberStr = sceneName.Substring(6);
                if (int.TryParse(levelNumberStr, out int levelNumber))
                {
                    return levelNumber >= 0 && levelNumber <= 13;
                }
            }

            return false;
        }

        private void CheckForNewPlayers()
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (!playersWithRadios.Contains(player.ActorNumber))
                {
                    Logger.LogInfo($"New player detected: {player.NickName} - spawning radio");
                    StartCoroutine(SpawnRadioForPlayer(player));
                }
            }
        }

        private void SpawnRadiosForAllPlayers()
        {
            if (!PhotonNetwork.IsConnected || !PhotonNetwork.IsMasterClient)
            {
                return;
            }

            Logger.LogInfo("Automatically spawning radios for all players...");

            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (!playersWithRadios.Contains(player.ActorNumber))
                {
                    StartCoroutine(SpawnRadioForPlayer(player));
                }
            }
        }

        private IEnumerator SpawnRadioForPlayer(Photon.Realtime.Player player)
        {
            SpawnRadioForPlayer(player);
            yield return new WaitForSeconds(0.1f);

            // Mark this player as having received radio
            playersWithRadios.Add(player.ActorNumber);
        }        private void SpawnRadioForPlayer(Photon.Realtime.Player player)
        {
            try
            {
                // Get player's character
                Character playerCharacter = GetPlayerCharacter(player);
                if (playerCharacter == null) return;

                // Calculate spawn position near player
                Vector3 spawnPosition = playerCharacter.Center + Vector3.up * 1f + UnityEngine.Random.insideUnitSphere * 1.5f;
                spawnPosition.y = Mathf.Max(spawnPosition.y, playerCharacter.Center.y);

                // Spawn radio using PEAKLib system
                ClimbTunesRadioItem.SpawnRadioItem(spawnPosition);
                
                Logger.LogInfo($"Radio spawned for player: {player.NickName}");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error spawning radio for {player.NickName}: {ex.Message}");
            }
        }

        private Character GetPlayerCharacter(Photon.Realtime.Player player)
        {
            foreach (var character in Character.AllCharacters)
            {
                if (character != null && character.photonView.Owner == player)
                {
                    return character;
                }
            }
            return null;
        }

        private GameObject CreateRadioGameObject(Vector3 position)
        {
            // Create radio GameObject
            GameObject radioObj = new GameObject("ClimbTunes_Radio");
            radioObj.transform.position = position;

            // Add radio component
            ClimbTunesRadio radio = radioObj.AddComponent<ClimbTunesRadio>();
            radio.Initialize(radioVolume.Value);

            // Add visual components
            MeshRenderer renderer = radioObj.AddComponent<MeshRenderer>();
            MeshFilter filter = radioObj.AddComponent<MeshFilter>();

            // Create a simple box mesh for the radio
            filter.mesh = CreateRadioMesh();

            // Add collider for interaction
            BoxCollider collider = radioObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.2f, 0.4f);

            // Add rigidbody for physics
            Rigidbody rb = radioObj.AddComponent<Rigidbody>();
            rb.mass = 1f;

            return radioObj;
        }

        private Mesh CreateRadioMesh()
        {
            // Create a simple box mesh for the radio
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
            DestroyImmediate(cube);
            return mesh;
        }        private void ToggleRadioUI()
        {
            if (isUIOpen)
            {
                CloseRadioUI();
            }
            else
            {
                OpenRadioUI();
            }
        }
        
        public void OpenRadioUI()
        {
            if (radioUI != null) return;
            
            // Create UI GameObject
            radioUI = new GameObject("ClimbTunes_UI");
            Canvas canvas = radioUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            CanvasScaler scaler = radioUI.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GraphicRaycaster raycaster = radioUI.AddComponent<GraphicRaycaster>();
            
            // Create UI panel
            CreateRadioUIPanel();
            
            isUIOpen = true;
            Logger.LogInfo("Radio UI opened");
        }

        private void CreateRadioUIPanel()
        {
            // Create main panel
            GameObject panel = new GameObject("RadioPanel");
            panel.transform.SetParent(radioUI.transform, false);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.3f, 0.3f);
            panelRect.anchorMax = new Vector2(0.7f, 0.7f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Create title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(panel.transform, false);

            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "ClimbTunes Radio";
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;

            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Create URL input field
            CreateURLInputField(panel);

            // Create control buttons
            CreateControlButtons(panel);

            // Create close button
            CreateCloseButton(panel);
        }

        private void CreateURLInputField(GameObject parent)
        {
            GameObject inputFieldObj = new GameObject("URLInput");
            inputFieldObj.transform.SetParent(parent.transform, false);

            Image inputBg = inputFieldObj.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            TMP_InputField inputField = inputFieldObj.AddComponent<TMP_InputField>();
            inputField.placeholder = CreatePlaceholderText(inputFieldObj, "Enter YouTube URL or playlist...");
            inputField.text = defaultPlaylist.Value;

            // Create text area for input
            GameObject textAreaObj = new GameObject("TextArea");
            textAreaObj.transform.SetParent(inputFieldObj.transform, false);

            RectTransform textAreaRect = textAreaObj.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 5);
            textAreaRect.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI inputText = textAreaObj.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 14;
            inputText.color = Color.white;

            inputField.textComponent = inputText;
            inputField.targetGraphic = inputBg;

            RectTransform inputRect = inputFieldObj.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.1f, 0.5f);
            inputRect.anchorMax = new Vector2(0.9f, 0.65f);
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
        }

        private TextMeshProUGUI CreatePlaceholderText(GameObject parent, string text)
        {
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(parent.transform, false);

            TextMeshProUGUI placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholder.text = text;
            placeholder.fontSize = 14;
            placeholder.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10, 5);
            placeholderRect.offsetMax = new Vector2(-10, -5);

            return placeholder;
        }

        private void CreateControlButtons(GameObject parent)
        {
            string[] buttonNames = { "Play", "Pause", "Stop", "Skip" };
            float buttonWidth = 0.2f;
            float spacing = 0.05f;

            for (int i = 0; i < buttonNames.Length; i++)
            {
                GameObject buttonObj = new GameObject(buttonNames[i] + "Button");
                buttonObj.transform.SetParent(parent.transform, false);

                Image buttonImage = buttonObj.AddComponent<Image>();
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

                Button button = buttonObj.AddComponent<Button>();
                button.targetGraphic = buttonImage;

                // Button text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);

                TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = buttonNames[i];
                buttonText.fontSize = 14;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;

                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                // Position button
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                float startX = 0.1f + i * (buttonWidth + spacing);
                buttonRect.anchorMin = new Vector2(startX, 0.3f);
                buttonRect.anchorMax = new Vector2(startX + buttonWidth, 0.4f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;

                // Add button functionality
                string buttonName = buttonNames[i];
                button.onClick.AddListener(() => OnControlButtonClick(buttonName));
            }
        }

        private void CreateCloseButton(GameObject parent)
        {
            GameObject closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(parent.transform, false);

            Image closeButtonImage = closeButtonObj.AddComponent<Image>();
            closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            Button closeButton = closeButtonObj.AddComponent<Button>();
            closeButton.targetGraphic = closeButtonImage;
            closeButton.onClick.AddListener(CloseRadioUI);

            // Close button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(closeButtonObj.transform, false);

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "X";
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Position close button
            RectTransform closeRect = closeButtonObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.9f, 0.9f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
        }

        private void OnControlButtonClick(string buttonName)
        {
            Logger.LogInfo($"Radio control clicked: {buttonName}");

            switch (buttonName)
            {
                case "Play":
                    // Handle play functionality
                    break;
                case "Pause":
                    // Handle pause functionality
                    break;
                case "Stop":
                    // Handle stop functionality
                    break;
                case "Skip":
                    // Handle skip functionality
                    break;
            }
        }

        private void CloseRadioUI()
        {
            if (radioUI != null)
            {
                DestroyImmediate(radioUI);
                radioUI = null;
            }

            isUIOpen = false;
            Logger.LogInfo("Radio UI closed");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            playersWithRadios.Clear();

            if (radioUI != null)
            {
                DestroyImmediate(radioUI);
            }
        }

        private void OnDisable()
        {
            CloseRadioUI();
        }
    }
}
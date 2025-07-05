using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zorro.Core;

namespace ClimbMap
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class ClimbMapPlugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        
        // Configuration
        private ConfigEntry<bool> modEnabled;
        private ConfigEntry<float> mapWidth;
        private ConfigEntry<float> mapHeight;
        private ConfigEntry<float> mapOpacity;
        private ConfigEntry<bool> showPlayerNames;
        
        // UI Components
        private GameObject minimapUI;
        private Canvas minimapCanvas;
        private RectTransform minimapContainer;
        private Dictionary<int, MinimapPlayerDot> playerDots = new Dictionary<int, MinimapPlayerDot>();
        private List<MinimapCampfireDot> campfireDots = new List<MinimapCampfireDot>();
        
        // Game references
        private float minAltitude = 0f;
        private float maxAltitude = 1000f;
        private List<Campfire> campfires = new List<Campfire>();
        private Campfire currentTargetCampfire = null;
        private GameObject currentTargetMarker = null;
        
        // State tracking
        private bool isInGameLevel = false;
        private string currentSceneName = "";
        
        private void Awake()
        {
            Logger = base.Logger;
            
            // Configuration setup
            modEnabled = Config.Bind("General", "ModEnabled", true, "Enable/disable the minimap");
            mapWidth = Config.Bind("Appearance", "MapWidth", 200f, "Width of the minimap");
            mapHeight = Config.Bind("Appearance", "MapHeight", 400f, "Height of the minimap");
            mapOpacity = Config.Bind("Appearance", "MapOpacity", 0.8f, "Opacity of the minimap background");
            showPlayerNames = Config.Bind("Appearance", "ShowPlayerNames", true, "Show player names on minimap");
            
            if (!modEnabled.Value)
            {
                Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is disabled!");
                return;
            }
            
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
        
        private void Start()
        {
            if (!modEnabled.Value) return;
            
            // Subscribe to scene change events
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Check current scene
            CheckCurrentScene();
        }
        
        private void Update()
        {
            if (!modEnabled.Value || !isInGameLevel) return;
            
            // Only update if we're in a valid game level and have UI
            if (minimapUI != null && minimapUI.activeSelf)
            {
                UpdateMinimap();
                UpdateCurrentTarget();
            }
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneName = scene.name;
            CheckCurrentScene();
        }
        
        private void CheckCurrentScene()
        {
            isInGameLevel = IsInValidGameLevel();
            
            if (isInGameLevel)
            {
                Logger.LogInfo($"Entered game level: {currentSceneName} - Initializing minimap");
                StartCoroutine(InitializeMinimapDelayed());
            }
            else
            {
                Logger.LogInfo($"Not in game level: {currentSceneName} - Hiding minimap");
                HideMinimap();
            }
        }
        
        private bool IsInValidGameLevel()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            
            // Check if it's a Level_X scene (Level_0 through Level_13)
            if (sceneName.StartsWith("Level_"))
            {
                string levelNumberStr = sceneName.Substring(6); // Remove "Level_" prefix
                if (int.TryParse(levelNumberStr, out int levelNumber))
                {
                    return levelNumber >= 0 && levelNumber <= 13;
                }
            }
            
            return false;
        }
        
        private IEnumerator InitializeMinimapDelayed()
        {
            // Wait a bit for the scene to fully load
            yield return new WaitForSeconds(1f);
            
            if (minimapUI == null)
            {
                CreateMinimapUI();
            }
            
            FindCampfires();
            CalculateAltitudeRange();
            ShowMinimap();
        }
        
        private void CreateMinimapUI()
        {
            // Create main canvas
            GameObject canvasObj = new GameObject("ClimbMap_Canvas");
            minimapCanvas = canvasObj.AddComponent<Canvas>();
            minimapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            minimapCanvas.sortingOrder = 1000;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create minimap container
            GameObject containerObj = new GameObject("MinimapContainer");
            containerObj.transform.SetParent(canvasObj.transform, false);
            
            minimapContainer = containerObj.AddComponent<RectTransform>();
            minimapContainer.anchorMin = new Vector2(1, 1);
            minimapContainer.anchorMax = new Vector2(1, 1);
            minimapContainer.pivot = new Vector2(1, 1);
            minimapContainer.anchoredPosition = new Vector2(-20, -20);
            minimapContainer.sizeDelta = new Vector2(mapWidth.Value, mapHeight.Value);
            
            // Add background
            Image background = containerObj.AddComponent<Image>();
            background.color = new Color(0, 0, 0, mapOpacity.Value);
            
            // Add border
            Outline outline = containerObj.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2, 2);
            
            // Create title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(containerObj.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(0, 30);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "CLIMB MAP";
            titleText.color = Color.white;
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 14;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;
            
            minimapUI = canvasObj;
            DontDestroyOnLoad(minimapUI);
        }
        
        private void FindCampfires()
        {
            // Clear existing campfire dots
            foreach (var dot in campfireDots)
            {
                if (dot != null) DestroyImmediate(dot.gameObject);
            }
            campfireDots.Clear();
            
            campfires.Clear();
            campfires.AddRange(FindObjectsOfType<Campfire>());
            
            // Sort campfires by altitude (lowest to highest)
            campfires = campfires.OrderBy(c => c.transform.position.y).ToList();
            
            foreach (var campfire in campfires)
            {
                CreateCampfireDot(campfire);
            }
            
            Logger.LogInfo($"Found {campfires.Count} campfires");
        }
        
        private void CalculateAltitudeRange()
        {
            if (campfires.Count == 0) return;
            
            minAltitude = campfires.Min(c => c.transform.position.y) - 100f;
            maxAltitude = campfires.Max(c => c.transform.position.y) + 100f;
            
            Logger.LogInfo($"Altitude range: {minAltitude} to {maxAltitude}");
        }
        
        private void CreateCampfireDot(Campfire campfire)
        {
            GameObject dotObj = new GameObject($"CampfireDot_{campfire.name}");
            dotObj.transform.SetParent(minimapContainer, false);
            
            RectTransform dotRect = dotObj.AddComponent<RectTransform>();
            dotRect.sizeDelta = new Vector2(12, 12);
            
            Image dotImage = dotObj.AddComponent<Image>();
            dotImage.color = GetCampfireColor(campfire);
            dotImage.sprite = CreateSquareSprite();
            
            MinimapCampfireDot campfireDot = dotObj.AddComponent<MinimapCampfireDot>();
            campfireDot.campfire = campfire;
            campfireDot.dotImage = dotImage;
            
            campfireDots.Add(campfireDot);
        }
        
        private Color GetCampfireColor(Campfire campfire)
        {
            if (campfire.Lit) return Color.red;
            if (campfire.state == Campfire.FireState.Spent) return Color.gray;
            return Color.white;
        }
        
        private void CreateCurrentTargetMarker()
        {
            if (currentTargetMarker != null) DestroyImmediate(currentTargetMarker);
            
            currentTargetMarker = new GameObject("CurrentTargetMarker");
            currentTargetMarker.transform.SetParent(minimapContainer, false);
            
            RectTransform markerRect = currentTargetMarker.AddComponent<RectTransform>();
            markerRect.sizeDelta = new Vector2(16, 16);
            
            Image markerImage = currentTargetMarker.AddComponent<Image>();
            markerImage.color = Color.yellow;
            markerImage.sprite = CreateTriangleSprite();
            
            // Add pulsing effect
            StartCoroutine(PulseTargetMarker(markerImage));
        }
        
        private IEnumerator PulseTargetMarker(Image markerImage)
        {
            while (markerImage != null)
            {
                float pulseSpeed = 2f;
                float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                alpha = Mathf.Lerp(0.5f, 1f, alpha);
                
                Color color = markerImage.color;
                color.a = alpha;
                markerImage.color = color;
                
                yield return null;
            }
        }
        
        private void UpdateCurrentTarget()
        {
            // Find the next unlit campfire (current target)
            Campfire newTarget = campfires.FirstOrDefault(c => !c.Lit && c.state != Campfire.FireState.Spent);
            
            if (newTarget != currentTargetCampfire)
            {
                currentTargetCampfire = newTarget;
                
                if (currentTargetCampfire != null)
                {
                    CreateCurrentTargetMarker();
                    Logger.LogInfo($"New target campfire: {currentTargetCampfire.name}");
                }
                else
                {
                    // All campfires are lit - game completed
                    if (currentTargetMarker != null)
                    {
                        DestroyImmediate(currentTargetMarker);
                        currentTargetMarker = null;
                    }
                }
            }
        }
        
        private Sprite CreateSquareSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
        
        private Sprite CreateTriangleSprite()
        {
            int size = 16;
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Create triangle pointing up
                    float centerX = size / 2f;
                    float normalizedY = (float)y / size;
                    float width = (1f - normalizedY) * size;
                    
                    if (x >= centerX - width/2 && x <= centerX + width/2 && y < size * 0.8f)
                    {
                        colors[y * size + x] = Color.white;
                    }
                    else
                    {
                        colors[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
        
        private Sprite CreateCircleSprite()
        {
            int size = 32;
            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    colors[y * size + x] = distance <= radius ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
        
        private void UpdateMinimap()
        {
            UpdatePlayerDots();
            UpdateCampfireDots();
            UpdateTargetMarkerPosition();
        }
        
        private void UpdatePlayerDots()
        {
            var allCharacters = Character.AllCharacters;
            var currentPlayerIds = new HashSet<int>();
            
            foreach (var character in allCharacters)
            {
                if (character == null || character.data.dead) continue;
                
                int playerId = character.photonView.Owner.ActorNumber;
                currentPlayerIds.Add(playerId);
                
                if (!playerDots.ContainsKey(playerId))
                {
                    CreatePlayerDot(character, playerId);
                }
                else
                {
                    UpdatePlayerDotPosition(playerDots[playerId]);
                }
            }
            
            // Remove dots for disconnected players
            var playersToRemove = playerDots.Keys.Where(id => !currentPlayerIds.Contains(id)).ToList();
            foreach (var playerId in playersToRemove)
            {
                if (playerDots[playerId] != null)
                {
                    DestroyImmediate(playerDots[playerId].gameObject);
                }
                playerDots.Remove(playerId);
            }
        }
        
        private void CreatePlayerDot(Character character, int playerId)
        {
            GameObject dotObj = new GameObject($"PlayerDot_{character.characterName}");
            dotObj.transform.SetParent(minimapContainer, false);
            
            RectTransform dotRect = dotObj.AddComponent<RectTransform>();
            dotRect.sizeDelta = new Vector2(8, 8);
            
            Image dotImage = dotObj.AddComponent<Image>();
            dotImage.color = character.refs.customization.PlayerColor;
            dotImage.sprite = CreateCircleSprite();
            
            MinimapPlayerDot playerDot = dotObj.AddComponent<MinimapPlayerDot>();
            playerDot.character = character;
            playerDot.dotImage = dotImage;
            
            // Add player name if enabled
            if (showPlayerNames.Value)
            {
                GameObject nameObj = new GameObject("PlayerName");
                nameObj.transform.SetParent(dotObj.transform, false);
                
                RectTransform nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 0);
                nameRect.anchorMax = new Vector2(1, 0);
                nameRect.pivot = new Vector2(0.5f, 1);
                nameRect.anchoredPosition = new Vector2(0, -2);
                nameRect.sizeDelta = new Vector2(80, 20);
                
                Text nameText = nameObj.AddComponent<Text>();
                nameText.text = character.characterName;
                nameText.color = Color.white;
                nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                nameText.fontSize = 10;
                nameText.alignment = TextAnchor.MiddleCenter;
                
                // Add outline for better visibility
                Outline nameOutline = nameObj.AddComponent<Outline>();
                nameOutline.effectColor = Color.black;
                nameOutline.effectDistance = new Vector2(1, 1);
                
                playerDot.nameText = nameText;
            }
            
            playerDots[playerId] = playerDot;
            UpdatePlayerDotPosition(playerDot);
        }
        
        private void UpdatePlayerDotPosition(MinimapPlayerDot playerDot)
        {
            if (playerDot == null || playerDot.character == null) return;
            
            Vector3 worldPos = playerDot.character.Center;
            float normalizedY = Mathf.InverseLerp(minAltitude, maxAltitude, worldPos.y);
            
            // Position on the vertical minimap
            float mapY = Mathf.Lerp(-mapHeight.Value / 2 + 40, mapHeight.Value / 2 - 40, normalizedY);
            
            // Add some horizontal variation based on player position for better visibility
            float mapX = Mathf.Sin(worldPos.x * 0.1f + worldPos.z * 0.1f) * 20f;
            
            RectTransform dotRect = playerDot.GetComponent<RectTransform>();
            dotRect.anchoredPosition = new Vector2(mapX, mapY);
            
            // Update color based on player state
            Color dotColor = playerDot.character.refs.customization.PlayerColor;
            if (playerDot.character.data.passedOut)
            {
                dotColor = Color.yellow;
            }
            else if (playerDot.character.data.dead)
            {
                dotColor = Color.red;
            }
            
            playerDot.dotImage.color = dotColor;
        }
        
        private void UpdateCampfireDots()
        {
            foreach (var campfireDot in campfireDots)
            {
                if (campfireDot == null || campfireDot.campfire == null) continue;
                
                Vector3 worldPos = campfireDot.campfire.transform.position;
                float normalizedY = Mathf.InverseLerp(minAltitude, maxAltitude, worldPos.y);
                
                float mapY = Mathf.Lerp(-mapHeight.Value / 2 + 40, mapHeight.Value / 2 - 40, normalizedY);
                
                RectTransform dotRect = campfireDot.GetComponent<RectTransform>();
                dotRect.anchoredPosition = new Vector2(0, mapY);
                
                // Update color based on campfire state
                campfireDot.dotImage.color = GetCampfireColor(campfireDot.campfire);
            }
        }
        
        private void UpdateTargetMarkerPosition()
        {
            if (currentTargetMarker == null || currentTargetCampfire == null) return;
            
            Vector3 worldPos = currentTargetCampfire.transform.position;
            float normalizedY = Mathf.InverseLerp(minAltitude, maxAltitude, worldPos.y);
            
            float mapY = Mathf.Lerp(-mapHeight.Value / 2 + 40, mapHeight.Value / 2 - 40, normalizedY);
            
            RectTransform markerRect = currentTargetMarker.GetComponent<RectTransform>();
            markerRect.anchoredPosition = new Vector2(25, mapY); // Offset to the right of campfire
        }
        
        private void ShowMinimap()
        {
            if (minimapUI != null)
            {
                minimapUI.SetActive(true);
                Logger.LogInfo("Minimap shown");
            }
        }
        
        private void HideMinimap()
        {
            if (minimapUI != null)
            {
                minimapUI.SetActive(false);
                Logger.LogInfo("Minimap hidden");
            }
        }
        
        private void OnDestroy()
        {
            if (minimapUI != null)
            {
                Destroy(minimapUI);
            }
            
            // Unsubscribe from events
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    public class MinimapPlayerDot : MonoBehaviour
    {
        public Character character;
        public Image dotImage;
        public Text nameText;
    }
    
    public class MinimapCampfireDot : MonoBehaviour
    {
        public Campfire campfire;
        public Image dotImage;
    }
}

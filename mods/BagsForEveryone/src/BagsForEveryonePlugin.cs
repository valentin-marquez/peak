using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Zorro.Core;

namespace BagsForEveryone
{
    /// <summary>
    /// Minimalist BepInEx plugin that automatically spawns backpacks for all players when starting a game.
    /// Only activates in actual game levels (Level_0 to Level_13), not in lobby or menu scenes.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class BagsForEveryonePlugin : BaseUnityPlugin
    {
        public static BagsForEveryonePlugin Instance { get; private set; }
        internal new static ManualLogSource Logger { get; private set; }

        // Single configuration option - enable/disable the mod
        private ConfigEntry<bool> _modEnabled;

        // Player tracking to prevent duplicate bag spawning
        private HashSet<int> _playersWithBags = new HashSet<int>();
        private bool _gameStarted = false;

        // Fixed configuration values for minimalist approach
        private const int BAG_AMOUNT = 1;
        private const float SPAWN_DELAY = 2f;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            // Only one configuration option - enable/disable
            _modEnabled = Config.Bind("General", "ModEnabled", true, "Enable or disable automatic backpack spawning");

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} loaded successfully!");
            Logger.LogInfo("Minimalist automatic backpack spawning system initialized");
        }

        private void Start()
        {
            // Subscribe to scene change events to reset state
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Start the main monitoring coroutine
            StartCoroutine(MonitorGameState());
        }

        /// <summary>
        /// Resets mod state when a new scene is loaded
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _gameStarted = false;
            _playersWithBags.Clear();
            Logger.LogInfo($"Scene changed to: {scene.name} - mod state reset");
        }

        /// <summary>
        /// Main monitoring loop that handles game state and player tracking
        /// </summary>
        private IEnumerator MonitorGameState()
        {
            while (true)
            {
                if (!_modEnabled.Value)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Only activate in actual game levels, not in Airport (lobby) or other scenes
                if (!IsInValidGameLevel())
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // Check if we're connected to Photon and game has started
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                {
                    if (!_gameStarted)
                    {
                        _gameStarted = true;
                        Logger.LogInfo($"Game started in level: {SceneManager.GetActiveScene().name} - spawning backpacks");
                        yield return new WaitForSeconds(SPAWN_DELAY);
                        SpawnBagsForAllPlayers();
                    }
                    else
                    {
                        // Always check for new players joining mid-game
                        CheckForNewPlayers();
                    }
                }
                else
                {
                    // Reset game state if disconnected
                    if (_gameStarted)
                    {
                        _gameStarted = false;
                        _playersWithBags.Clear();
                        Logger.LogInfo("Game session ended - resetting bag tracking");
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Validates if we're currently in a game level where bags should be spawned.
        /// Excludes lobby (Airport) and menu scenes, only allows Level_0 through Level_13.
        /// </summary>
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

        /// <summary>
        /// Checks for new players who joined mid-game and need bags
        /// </summary>
        private void CheckForNewPlayers()
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (!_playersWithBags.Contains(player.ActorNumber))
                {
                    Logger.LogInfo($"New player detected: {player.NickName} - spawning backpack");
                    StartCoroutine(SpawnBagsForPlayer(player));
                }
            }
        }

        /// <summary>
        /// Spawns bags for all players in the current game session.
        /// Only executed by the master client to prevent duplication.
        /// </summary>
        private void SpawnBagsForAllPlayers()
        {
            if (!PhotonNetwork.IsConnected || !PhotonNetwork.IsMasterClient)
            {
                return;
            }

            Logger.LogInfo("Automatically spawning backpacks for all players...");

            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (!_playersWithBags.Contains(player.ActorNumber))
                {
                    StartCoroutine(SpawnBagsForPlayer(player));
                }
            }
        }

        /// <summary>
        /// Spawns a backpack for a specific player
        /// </summary>
        private IEnumerator SpawnBagsForPlayer(Photon.Realtime.Player player)
        {
            SpawnBagForPlayer(player);
            yield return new WaitForSeconds(0.1f);
            
            // Mark this player as having received bags
            _playersWithBags.Add(player.ActorNumber);
        }

        /// <summary>
        /// Spawns a single bag for the specified player.
        /// Attempts to give directly to backpack slot first, then spawns near player as fallback.
        /// </summary>
        private void SpawnBagForPlayer(Photon.Realtime.Player player)
        {
            try
            {
                // Get player's character and Player component
                Character playerCharacter = GetPlayerCharacter(player);
                if (playerCharacter == null) return;

                Player playerComponent = GetPlayerComponent(player);
                if (playerComponent == null) return;

                // Check if player already has a backpack
                if (playerComponent.backpackSlot.hasBackpack)
                {
                    Logger.LogInfo($"Player {player.NickName} already has a backpack, skipping");
                    return;
                }

                // Try to give backpack directly to player's backpack slot
                if (TryGiveBackpackToPlayer(playerComponent))
                {
                    Logger.LogInfo($"Backpack given to player: {player.NickName}");
                }
                else
                {
                    // Fallback: spawn a backpack near the player
                    SpawnBackpackNearPlayer(playerCharacter);
                    Logger.LogInfo($"Backpack spawned near player: {player.NickName}");
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error spawning backpack for {player.NickName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds the Character component for a specific Photon player
        /// </summary>
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

        /// <summary>
        /// Finds the Player component for a specific Photon player
        /// </summary>
        private Player GetPlayerComponent(Photon.Realtime.Player photonPlayer)
        {
            var playerObjects = FindObjectsByType<Player>(FindObjectsSortMode.None);
            foreach (var player in playerObjects)
            {
                if (player.photonView.Owner == photonPlayer)
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to give a backpack directly to the player's backpack slot
        /// </summary>
        private bool TryGiveBackpackToPlayer(Player player)
        {
            try
            {
                ushort backpackItemID = FindBackpackItemID();
                if (backpackItemID == 0) return false;

                // Use the game's official Player.AddItem method
                ItemSlot slot;
                bool success = player.AddItem(backpackItemID, null, out slot);
                
                return success && slot is BackpackSlot;
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error giving backpack to player: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Finds the item ID for backpacks in the game's ItemDatabase
        /// </summary>
        private ushort FindBackpackItemID()
        {
            try
            {
                // Try to find backpack in the ItemDatabase
                var itemDatabase = SingletonAsset<ItemDatabase>.Instance;
                if (itemDatabase?.itemLookup != null)
                {
                    foreach (var kvp in itemDatabase.itemLookup)
                    {
                        if (kvp.Value is Backpack)
                        {
                            return kvp.Key;
                        }
                    }
                }
                
                // Fallback: search for existing Backpack components in scene
                var allBackpacks = FindObjectsByType<Backpack>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (allBackpacks.Length > 0)
                {
                    return allBackpacks[0].itemID;
                }
                
                return 0;
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error finding backpack item ID: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Spawns a backpack item near the specified character as a fallback method
        /// </summary>
        private void SpawnBackpackNearPlayer(Character character)
        {
            try
            {
                // Calculate spawn position near player
                Vector3 spawnPosition = character.Center + Vector3.up * 1f + Random.insideUnitSphere * 1.5f;
                spawnPosition.y = Mathf.Max(spawnPosition.y, character.Center.y); // Prevent underground spawning

                string backpackPrefabName = FindBackpackPrefabName();
                if (string.IsNullOrEmpty(backpackPrefabName)) return;

                // Spawn using PhotonNetwork for multiplayer synchronization
                PhotonNetwork.InstantiateItemRoom(backpackPrefabName, spawnPosition, Quaternion.identity);
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error spawning backpack near player: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds the prefab name for backpacks to use with PhotonNetwork.InstantiateItemRoom
        /// </summary>
        private string FindBackpackPrefabName()
        {
            try
            {
                // Try to find backpack prefab from ItemDatabase
                var itemDatabase = SingletonAsset<ItemDatabase>.Instance;
                if (itemDatabase?.itemLookup != null)
                {
                    foreach (var kvp in itemDatabase.itemLookup)
                    {
                        if (kvp.Value is Backpack)
                        {
                            return kvp.Value.gameObject.name;
                        }
                    }
                }
                
                // Fallback: search for existing backpack objects in scene
                var allBackpacks = FindObjectsByType<Backpack>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (allBackpacks.Length > 0)
                {
                    return allBackpacks[0].gameObject.name;
                }
                
                return null;
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error finding backpack prefab name: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Cleanup when plugin is destroyed
        /// </summary>
        private void OnDestroy()
        {
            // Unsubscribe from scene change events
            SceneManager.sceneLoaded -= OnSceneLoaded;
            
            _playersWithBags.Clear();
        }
    }
}

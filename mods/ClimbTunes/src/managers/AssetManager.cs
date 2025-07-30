using BepInEx;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ClimbTunes.Utils;
using Photon.Pun;

namespace ClimbTunes.Manager
{
    public class AssetManager
    {
        private readonly BaseUnityPlugin plugin;
        private AssetBundle assetBundle;
        private GameObject radioPrefab;
        private GameObject playlistItemPrefab;
        private const string RADIO_PREFAB_NAME = "Radio";
        
        // Dictionary para cachear los iconos
        private Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();
        
        // Instancia estática para acceso global
        private static AssetManager instance;

        public AssetManager(BaseUnityPlugin plugin)
        {
            this.plugin = plugin;
            instance = this; // Asignar la instancia estática
        }

        public void LoadAssetBundle()
        {
            try
            {
                PhotonPoolManager.Initialize(plugin);

                string assetBundlePath = PathUtils.GetAssetBundlePath();
                ClimbTunes.ModLogger.LogInfo($"Loading AssetBundle from: {assetBundlePath}");

                if (!File.Exists(assetBundlePath))
                {
                    ClimbTunes.ModLogger.LogError($"AssetBundle not found at: {assetBundlePath}");
                    return;
                }

                assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
                
                if (assetBundle == null)
                {
                    ClimbTunes.ModLogger.LogError("Failed to load AssetBundle");
                    return;
                }

                // Cargar ambos prefabs directamente
                radioPrefab = assetBundle.LoadAsset<GameObject>("assets/climbtunes/prefabs/radio.prefab");
                playlistItemPrefab = assetBundle.LoadAsset<GameObject>("assets/climbtunes/prefabs/playlistitem.prefab");
                
                if (radioPrefab == null)
                {
                    ClimbTunes.ModLogger.LogError("Failed to load Radio prefab from AssetBundle");
                    return;
                }
                
                if (playlistItemPrefab == null)
                {
                    ClimbTunes.ModLogger.LogError("Failed to load PlaylistItem prefab from AssetBundle");
                }

                // Configurar el prefab para uso en el juego
                PrepareRadioPrefab();
                
                // Apply font when ready
                plugin.StartCoroutine(FontManager.WaitAndApplyGameFont(radioPrefab));

                // Cargar iconos
                LoadItemIcons();

                if (ValidateRadioPrefab())
                {
                    ClimbTunes.ModLogger.LogInfo("Radio prefab loaded and validated successfully");
                    RegisterRadioPrefab();
                    RegisterInItemDatabase();
                }
                else
                {
                    ClimbTunes.ModLogger.LogError("Radio prefab validation failed");
                }
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error loading AssetBundle: {ex.Message}");
            }
        }

        private void PrepareRadioPrefab()
        {
            try
            {
                // Asegurarse de que el nombre es correcto
                radioPrefab.name = RADIO_PREFAB_NAME;
                
                // Asegurarse de que no se destruya al cambiar de escena
                Object.DontDestroyOnLoad(radioPrefab);
                
                // Desactivar el objeto para evitar que se ejecute Start() prematuramente
                radioPrefab.SetActive(false);

                // Font patching is handled separately via coroutine

                // Log de componentes existentes
                LogExistingComponents(radioPrefab);

                ClimbTunes.ModLogger.LogInfo("Radio prefab prepared successfully");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error preparing radio prefab: {ex.Message}");
            }
        }

        private void LogExistingComponents(GameObject prefab)
        {
            ClimbTunes.ModLogger.LogInfo($"=== Components in Radio prefab ===");
            Component[] components = prefab.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp != null)
                {
                    ClimbTunes.ModLogger.LogInfo($"- {comp.GetType().Name}");
                }
            }
            ClimbTunes.ModLogger.LogInfo($"=== End of components list ===");
        }

        private void LoadItemIcons()
        {
            try
            {
                // Cargar el icono de Radio
                Texture2D radioIcon = assetBundle.LoadAsset<Texture2D>("assets/climbtunes/texture2d/radio.png");
                if (radioIcon != null)
                {
                    iconCache["Radio"] = radioIcon;
                    ClimbTunes.ModLogger.LogInfo("Radio icon loaded successfully");
                }
                else
                {
                    ClimbTunes.ModLogger.LogError("Failed to load Radio icon from AssetBundle");
                }

            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error loading item icons: {ex.Message}");
            }
        }

        private bool ValidateRadioPrefab()
        {
            if (radioPrefab == null)
            {
                ClimbTunes.ModLogger.LogError("Radio prefab is null");
                return false;
            }

            // Verificar componentes básicos
            Item itemComponent = radioPrefab.GetComponent<Item>();
            PhotonView photonView = radioPrefab.GetComponent<PhotonView>();
            
            ClimbTunes.ModLogger.LogInfo("=== Validation Results ===");
            ClimbTunes.ModLogger.LogInfo($"Has Item component: {itemComponent != null}");
            ClimbTunes.ModLogger.LogInfo($"Has PhotonView component: {photonView != null}");
            
            if (!itemComponent)
            {
                ClimbTunes.ModLogger.LogError("Radio prefab missing Item component");
                return false;
            }
            
            if (!photonView)
            {
                ClimbTunes.ModLogger.LogError("Radio prefab missing PhotonView component");
                return false;
            }

            ClimbTunes.ModLogger.LogInfo("=== Validation Complete ===");
            return true;
        }

        private void RegisterRadioPrefab()
        {
            try
            {
                // Asegurarse de que el nombre es correcto
                if (radioPrefab.name != RADIO_PREFAB_NAME)
                {
                    radioPrefab.name = RADIO_PREFAB_NAME;
                }

                PhotonPoolManager.RegisterCustomPrefab(RADIO_PREFAB_NAME, radioPrefab);
                ClimbTunes.ModLogger.LogInfo($"Radio prefab registered in Photon pool as '{RADIO_PREFAB_NAME}'");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error registering radio prefab: {ex.Message}");
            }
        }

        private void RegisterInItemDatabase()
        {
            try
            {
                ItemDatabaseManager.RegisterRadioItem(radioPrefab);
                ClimbTunes.ModLogger.LogInfo("Radio item registered in ItemDatabase");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error registering item in database: {ex.Message}");
            }
        }

        // Métodos de instancia (usados por SpawnerManager)
        public GameObject GetRadioPrefab()
        {
            return radioPrefab;
        }

        public GameObject GetPlaylistItemPrefab()
        {
            return playlistItemPrefab;
        }

        public string GetRadioPrefabName()
        {
            return RADIO_PREFAB_NAME;
        }

        // Método para obtener iconos de items
        public Texture2D GetItemIcon(string itemName)
        {
            if (iconCache.TryGetValue(itemName, out Texture2D icon))
            {
                return icon;
            }

            ClimbTunes.ModLogger.LogWarning($"Icon not found for item: {itemName}");
            return null;
        }

        // Método estático para acceso global
        public static Texture2D GetStaticItemIcon(string itemName)
        {
            return instance?.GetItemIcon(itemName);
        }

        public static GameObject GetStaticPlaylistItemPrefab()
        {
            return instance?.playlistItemPrefab;
        }

        public static GameObject GetStaticRadioPrefab()
        {
            return instance?.radioPrefab;
        }

        // Método para obtener todos los iconos disponibles (útil para debugging)
        public string[] GetAvailableIcons()
        {
            var keys = new string[iconCache.Count];
            iconCache.Keys.CopyTo(keys, 0);
            return keys;
        }


        public void Cleanup()
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(false);
                assetBundle = null;
                ClimbTunes.ModLogger.LogInfo("AssetBundle unloaded");
            }
            
            // Limpiar cache de iconos
            iconCache.Clear();
            
            // Limpiar la instancia estática
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
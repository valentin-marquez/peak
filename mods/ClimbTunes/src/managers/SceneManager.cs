using BepInEx;
using UnityEngine;
using ClimbTunes.Utils;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace ClimbTunes.Manager
{
    public class SceneManager
    {
        private readonly BaseUnityPlugin plugin;
        private readonly AssetManager assetManager;
        private readonly SpawnerManager spawnerManager;

        public SceneManager(BaseUnityPlugin plugin, AssetManager assetManager, SpawnerManager spawnerManager)
        {
            this.plugin = plugin;
            this.assetManager = assetManager;
            this.spawnerManager = spawnerManager;
        }

        public void HandleSceneLoaded(UnityScene scene)
        {
            if (SceneUtils.IsLevelScene(scene.name))
            {
                ClimbTunes.ModLogger.LogInfo($"Level scene detected: {scene.name}");
                plugin.StartCoroutine(DelayedSceneSetup(scene));
            }
        }

        private System.Collections.IEnumerator DelayedSceneSetup(UnityScene scene)
        {
            yield return null;
            SetupRadioSpawner(scene);
        }

        private void SetupRadioSpawner(UnityScene scene)
        {
            try
            {
                GameObject crashedPlane = FindCrashedPlane();
                
                if (crashedPlane == null)
                {
                    ClimbTunes.ModLogger.LogWarning($"'crashed plane' not found in scene {scene.name}");
                    return;
                }

                // Verificar si ya existe un spawner
                if (crashedPlane.transform.Find("Radio_Spawner") != null)
                {
                    ClimbTunes.ModLogger.LogInfo("Radio_Spawner already exists, skipping creation");
                    return;
                }

                spawnerManager.CreateRadioSpawner(crashedPlane, assetManager.GetRadioPrefab());
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error setting up radio spawner: {ex.Message}");
            }
        }

        private GameObject FindCrashedPlane()
        {
            // Buscar directamente en la jerarquía conocida
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            
            foreach (GameObject rootObject in rootObjects)
            {
                if (rootObject.name == "Map")
                {
                    Transform beach = rootObject.transform.Find("Beach");
                    if (beach != null)
                    {
                        Transform crashedPlane = beach.Find("crashed plane");
                        if (crashedPlane != null)
                        {
                            return crashedPlane.gameObject;
                        }
                    }
                }
            }
            
            return null;
        }
    }
}
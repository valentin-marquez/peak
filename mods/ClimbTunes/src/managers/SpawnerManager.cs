using BepInEx;
using UnityEngine;
using System.Collections;
using Photon.Pun;

namespace ClimbTunes.Manager
{
    public class SpawnerManager
    {
        private readonly BaseUnityPlugin plugin;
        private readonly AssetManager assetManager;
        private static readonly Vector3 RADIO_LOCAL_POSITION = new Vector3(-11.0139999f, 1.25100005f, -5.78299999f);
        private static readonly Vector3 RADIO_LOCAL_ROTATION = new Vector3(75.7733154f, 219.509323f, 322.810486f);

        public SpawnerManager(BaseUnityPlugin plugin, AssetManager assetManager)
        {
            this.plugin = plugin;
            this.assetManager = assetManager;
        }

        public void CreateRadioSpawner(GameObject parent, GameObject radioPrefab)
        {
            if (radioPrefab == null || parent == null)
            {
                ClimbTunes.ModLogger.LogError("Cannot create spawner: null prefab or parent");
                return;
            }

            try
            {
                // Crear el spawner
                GameObject radioSpawner = new GameObject("Radio_Spawner");
                radioSpawner.transform.SetParent(parent.transform);
                radioSpawner.transform.localPosition = RADIO_LOCAL_POSITION;
                radioSpawner.transform.localRotation = Quaternion.Euler(RADIO_LOCAL_ROTATION);

                // Configurar el componente spawner
                SingleItemSpawner spawner = radioSpawner.AddComponent<SingleItemSpawner>();
                spawner.isKinematic = false;
                spawner.prefab = radioPrefab;
                spawner.playersInRoomRequirement = 0;
                spawner.belowAscentRequirement = -1;

                // IMPORTANTE: Configurar el prefab name para que Photon lo encuentre
                spawner.prefab.name = assetManager.GetRadioPrefabName();

                ClimbTunes.ModLogger.LogInfo($"Radio spawner created successfully with prefab name: {spawner.prefab.name}");

                // Spawn inicial usando el método correcto del spawner
                plugin.StartCoroutine(DelayedSpawn(spawner));
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error creating radio spawner: {ex.Message}");
            }
        }

        private IEnumerator DelayedSpawn(SingleItemSpawner spawner)
        {
            yield return new WaitForSeconds(2f);

            try
            {
                if (spawner?.prefab == null)
                {
                    ClimbTunes.ModLogger.LogError("Cannot spawn: spawner or prefab is null");
                    yield break;
                }

                ClimbTunes.ModLogger.LogInfo("Attempting to spawn radio...");
                
                // Usar el método TrySpawnItems() del spawner en lugar de Object.Instantiate
                var spawnedItems = spawner.TrySpawnItems();
                
                if (spawnedItems != null && spawnedItems.Count > 0)
                {
                    ClimbTunes.ModLogger.LogInfo($"Radio spawned successfully. Items spawned: {spawnedItems.Count}");
                    
                    // Log de posición para debug
                    foreach (var item in spawnedItems)
                    {
                        if (item != null)
                        {
                            ClimbTunes.ModLogger.LogInfo($"Spawned item position: {item.transform.position}, local position: {item.transform.localPosition}");
                        }
                    }
                }
                else
                {
                    ClimbTunes.ModLogger.LogError("Failed to spawn radio - no items returned");
                }
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error during spawn: {ex.Message}");
            }
        }
    }
}
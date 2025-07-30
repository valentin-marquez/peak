using UnityEngine;
using Photon.Pun;
using BepInEx;
using System.Collections;
using System.Collections.Generic;
using ClimbTunes.Components;

namespace ClimbTunes.Manager
{
    public class CustomPrefabPool : IPunPrefabPool
    {
        private readonly IPunPrefabPool defaultPool;
        private readonly Dictionary<string, GameObject> customPrefabs = new Dictionary<string, GameObject>();
        private readonly BaseUnityPlugin plugin;

        public CustomPrefabPool(IPunPrefabPool defaultPool, BaseUnityPlugin plugin = null)
        {
            this.defaultPool = defaultPool;
            this.plugin = plugin;
        }

        public void RegisterCustomPrefab(string prefabName, GameObject prefab)
        {
            if (prefab == null)
            {
                ClimbTunes.ModLogger.LogError($"Cannot register null prefab: {prefabName}");
                return;
            }

            // Registrar tanto con el nombre simple como con el path completo
            customPrefabs[prefabName] = prefab;
            customPrefabs[$"0_Items/{prefabName}"] = prefab;
            
            ClimbTunes.ModLogger.LogInfo($"Custom prefab registered: {prefabName} and 0_Items/{prefabName}");
        }

        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
        {
            // Solo logear para prefabs de ClimbTunes
            bool isClimbTunesPrefab = prefabId.Contains("Radio") || customPrefabs.ContainsKey(prefabId);
            
            if (isClimbTunesPrefab)
            {
                ClimbTunes.ModLogger.LogInfo($"Attempting to instantiate ClimbTunes prefab: {prefabId}");
            }

            // Primero intentar con nuestros prefabs personalizados
            if (customPrefabs.TryGetValue(prefabId, out GameObject customPrefab))
            {
                ClimbTunes.ModLogger.LogInfo($"Using custom prefab for: {prefabId}");
                
                // Crear la instancia
                GameObject instance = Object.Instantiate(customPrefab, position, rotation);
                
                // Activar el objeto si estaba desactivado
                if (!instance.activeInHierarchy)
                {
                    instance.SetActive(true);
                }
                
                // Verificar que los componentes están presentes después de la instanciación
                VerifyInstanceComponents(instance, prefabId);
                
                return instance;
            }

            // Si no es nuestro prefab, usar el pool por defecto (sin logging)
            return defaultPool.Instantiate(prefabId, position, rotation);
        }

        private void VerifyInstanceComponents(GameObject instance, string prefabId)
        {
            if (prefabId.Contains("Radio"))
            {
                // Verificar componentes críticos sin listar todos
                Item itemComponent = instance.GetComponent<Item>();
                var radioController = instance.GetComponent<RadioController>();
                
                if (itemComponent == null)
                {
                    ClimbTunes.ModLogger.LogWarning($"Radio instance missing Item component: {prefabId}");
                }
                
                if (radioController == null)
                {
                    ClimbTunes.ModLogger.LogWarning($"Radio instance missing RadioController component: {prefabId}");
                }
                else
                {
                    ClimbTunes.ModLogger.LogInfo($"Radio {prefabId} instantiated successfully with all components");
                }

                // Apply font to runtime instances
                if (plugin != null)
                {
                    plugin.StartCoroutine(FontManager.WaitAndApplyGameFont(instance));
                }
                else
                {
                    // Fallback: try immediate application
                    FontManager.ApplyGameFontToPrefab(instance);
                }
            }
        }

        public void Destroy(GameObject gameObject)
        {
            // Para objetos personalizados, usar destroy normal
            // Para objetos del pool por defecto, usar su método destroy
            if (gameObject != null)
            {
                // Verificar si es uno de nuestros prefabs personalizados
                bool isCustomPrefab = false;
                foreach (var kvp in customPrefabs)
                {
                    if (gameObject.name.StartsWith(kvp.Value.name))
                    {
                        isCustomPrefab = true;
                        break;
                    }
                }

                if (isCustomPrefab)
                {
                    ClimbTunes.ModLogger.LogInfo($"Destroying custom prefab: {gameObject.name}");
                    Object.Destroy(gameObject);
                }
                else
                {
                    defaultPool.Destroy(gameObject);
                }
            }
        }


        public bool HasCustomPrefab(string prefabId)
        {
            return customPrefabs.ContainsKey(prefabId);
        }
    }
}
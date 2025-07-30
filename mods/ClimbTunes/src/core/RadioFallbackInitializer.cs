using UnityEngine;
using System.Collections;

namespace ClimbTunes.Core
{
    /// <summary>
    /// Sistema de fallback que verifica periódicamente si hay radios sin inicializar
    /// </summary>
    public class RadioFallbackInitializer : MonoBehaviour
    {
        private const float CHECK_INTERVAL = 5.0f; // Verificar cada 5 segundos
        private Coroutine checkCoroutine;

        private void Start()
        {
            // Iniciar verificación periódica
            checkCoroutine = StartCoroutine(PeriodicRadioCheck());
            ClimbTunes.ModLogger.LogInfo("RadioFallbackInitializer started periodic checks");
        }

        private IEnumerator PeriodicRadioCheck()
        {
            while (true)
            {
                yield return new WaitForSeconds(CHECK_INTERVAL);
                CheckForUninitializedRadios();
            }
        }

        private void CheckForUninitializedRadios()
        {
            try
            {
                // Buscar todos los GameObjects activos en la escena
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                
                foreach (GameObject obj in allObjects)
                {
                    if (IsRadioObject(obj) && !HasRadioInitializer(obj))
                    {
                        // Añadir inicializador a radio no inicializada
                        RadioComponentInitializer initializer = obj.AddComponent<RadioComponentInitializer>();
                        ClimbTunes.ModLogger.LogInfo($"Fallback: RadioComponentInitializer added to {obj.name}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error in fallback radio check: {ex.Message}");
            }
        }

        private bool IsRadioObject(GameObject obj)
        {
            if (obj == null) return false;

            try
            {
                // Verificar si es nuestro objeto de radio
                if (obj.name.Contains("Radio") || obj.name.Contains("climbtunes"))
                {
                    // Verificar que tenga la estructura esperada (Canvas hijo)
                    Transform canvas = obj.transform.Find("Canvas");
                    if (canvas != null)
                    {
                        // Verificar que tenga el RadioPanel
                        Transform radioPanel = canvas.Find("RadioPanel");
                        return radioPanel != null;
                    }
                }
            }
            catch (System.Exception)
            {
                // Ignorar errores silenciosamente en el fallback
            }
            return false;
        }

        private bool HasRadioInitializer(GameObject obj)
        {
            return obj.GetComponent<RadioComponentInitializer>() != null;
        }

        private void OnDestroy()
        {
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
            }
        }

        /// <summary>
        /// Crear una instancia de este fallback en la escena
        /// </summary>
        public static void CreateFallbackInitializer()
        {
            // Verificar si ya existe uno
            if (FindObjectOfType<RadioFallbackInitializer>() != null)
            {
                return; // Ya existe
            }

            // Crear GameObject persistente para el fallback
            GameObject fallbackGO = new GameObject("RadioFallbackInitializer");
            fallbackGO.AddComponent<RadioFallbackInitializer>();
            
            // Hacer que persista entre escenas
            DontDestroyOnLoad(fallbackGO);
            
            ClimbTunes.ModLogger.LogInfo("RadioFallbackInitializer created");
        }
    }
}
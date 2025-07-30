using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClimbTunes.Utils;

namespace ClimbTunes.Components
{
    /// <summary>
    /// Componente que se agrega automáticamente al Canvas de la radio para manejar la UI
    /// </summary>
    public class RadioUIManager : MonoBehaviour
    {
        private Canvas canvas;
        private GraphicRaycaster raycaster;
        private bool wasUIActive = false;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            raycaster = GetComponent<GraphicRaycaster>();
            
            // Configurar el Canvas para interacción
            SetupCanvas();
        }

        private void SetupCanvas()
        {
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
            }

            if (raycaster == null)
            {
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }

            // Asegurar que existe EventSystem
            if (EventSystem.current == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }
        }

        private void OnEnable()
        {
            wasUIActive = true;
            EnableCursor();
            ClimbTunes.ModLogger.LogInfo("Radio UI enabled - cursor activated");
            
            // Run debug test when UI is activated
            GameObject radioObject = transform.parent?.gameObject;
            if (radioObject != null)
            {
                ClimbTunes.ModLogger.LogInfo("Running button functionality test on UI activation...");
                RadioDebugUtils.TestButtonFunctionality(radioObject);
            }
        }

        private void OnDisable()
        {
            wasUIActive = false;
            DisableCursor();
            ClimbTunes.ModLogger.LogInfo("Radio UI disabled - cursor deactivated");
        }

        private void EnableCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void DisableCursor()
        {
            // Solo ocultar cursor si no hay otras radios con UI activa
            if (!HasOtherActiveRadioUI())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private bool HasOtherActiveRadioUI()
        {
            // Buscar otras radios con UI activa
            RadioUIManager[] allRadioUIs = FindObjectsOfType<RadioUIManager>();
            foreach (RadioUIManager radioUI in allRadioUIs)
            {
                if (radioUI != this && radioUI.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Método para cerrar la UI desde botones del Canvas
        /// </summary>
        public void CloseUI()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Verifica si esta UI está activa
        /// </summary>
        public bool IsUIActive()
        {
            return wasUIActive && gameObject.activeInHierarchy;
        }
    }
}
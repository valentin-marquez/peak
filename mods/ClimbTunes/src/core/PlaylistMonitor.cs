using UnityEngine;
using ClimbTunes.Components;

namespace ClimbTunes.Core
{
    /// <summary>
    /// Componente que monitorea cuando se añaden nuevos playlist items
    /// y automáticamente les agrega el PlaylistItemController
    /// </summary>
    public class PlaylistMonitor : MonoBehaviour
    {
        private int lastChildCount = 0;

        private void Start()
        {
            lastChildCount = transform.childCount;
            CheckExistingChildren();
        }

        private void Update()
        {
            // Verificar si se añadieron nuevos hijos
            if (transform.childCount != lastChildCount)
            {
                CheckForNewPlaylistItems();
                lastChildCount = transform.childCount;
            }
        }

        private void CheckExistingChildren()
        {
            // Verificar todos los hijos existentes
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                EnsurePlaylistItemController(child.gameObject);
            }
        }

        private void CheckForNewPlaylistItems()
        {
            // Verificar solo los nuevos hijos
            for (int i = lastChildCount; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                EnsurePlaylistItemController(child.gameObject);
            }
        }

        private void EnsurePlaylistItemController(GameObject playlistItem)
        {
            // Verificar si es un playlist item válido
            if (!IsPlaylistItem(playlistItem)) return;

            // Añadir PlaylistItemController si no existe
            PlaylistItemController controller = playlistItem.GetComponent<PlaylistItemController>();
            if (controller == null)
            {
                controller = playlistItem.AddComponent<PlaylistItemController>();
                ClimbTunes.ModLogger.LogInfo($"PlaylistItemController added to {playlistItem.name}");
            }
        }

        private bool IsPlaylistItem(GameObject obj)
        {
            // Verificar que tenga la estructura esperada de un playlist item
            if (obj.name.Contains("PlaylistItem") || obj.name.Contains("Item"))
            {
                // Verificar que tenga componentes esperados
                Transform trackText = obj.transform.Find("TrackText");
                Transform playButton = obj.transform.Find("PlayButton");
                
                return trackText != null || playButton != null;
            }
            return false;
        }

        /// <summary>
        /// Método público para forzar una verificación
        /// </summary>
        public void ForceCheck()
        {
            CheckExistingChildren();
            lastChildCount = transform.childCount;
        }
    }
}
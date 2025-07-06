using UnityEngine;
using System.Collections.Generic;
using Zorro.Core;
using Photon.Pun;
using Random = UnityEngine.Random;

namespace ClimbTunes
{
    /// <summary>
    /// Radio item management - handles creation, registration, and spawning of radio items
    /// Similar to BagsForEveryone approach but for radios
    /// </summary>
    public static class ClimbTunesRadioItem
    {
        private static GameObject radioItemPrefab;
        private static Item radioItemComponent;
        private static ushort radioItemID;
        private static bool isInitialized = false;
        
        /// <summary>
        /// Initialize the radio item and register it with the game
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;
            
            try
            {
                // Create radio GameObject with proper Item component
                radioItemPrefab = CreateRadioGameObject();
                radioItemComponent = radioItemPrefab.GetComponent<Item>();
                
                // Register with ItemDatabase (will be handled by game automatically)
                ClimbTunesPlugin.Logger.LogInfo("Radio item prefab created and ready for spawning");
                
                isInitialized = true;
            }
            catch (System.Exception ex)
            {
                ClimbTunesPlugin.Logger.LogError($"Failed to initialize radio item: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Create the radio GameObject with all necessary components
        /// This will eventually be replaced with asset bundle loading
        /// </summary>
        private static GameObject CreateRadioGameObject()
        {
            // Create base GameObject
            GameObject radioObj = new GameObject("ClimbTunes_Radio");
            radioObj.layer = LayerMask.NameToLayer("Item"); // Important: set to Item layer
            
            // Add Item component (required for all game items)
            Item itemComponent = radioObj.AddComponent<Item>();
            
            // Generate unique item ID (using hash of name to ensure consistency)
            radioItemID = (ushort)("ClimbTunes_Radio".GetHashCode() & 0xFFFF);
            itemComponent.itemID = radioItemID;
            
            // Configure Item UI Data
            itemComponent.UIData = new Item.ItemUIData
            {
                itemName = "ClimbTunes Radio",
                mainInteractPrompt = "Use Radio",
                hasMainInteract = true,
                hasSecondInteract = true,
                secondaryInteractPrompt = "Open Interface",
                canDrop = true,
                canPocket = true,
                canThrow = false,
                isShootable = false
            };
            
            // Set item properties
            itemComponent.mass = 2f;
            itemComponent.usingTimePrimary = 0.5f;
            itemComponent.showUseProgress = false;
            
            // Add radio functionality component
            ClimbTunesRadio radioComponent = radioObj.AddComponent<ClimbTunesRadio>();
            
            // Create visual components
            CreateRadioVisuals(radioObj);
            
            // Add physics components
            AddPhysicsComponents(radioObj);
            
            // Set up item interactions
            SetupItemInteractions(itemComponent, radioComponent);
            
            // Add PhotonView for networking
            PhotonView photonView = radioObj.AddComponent<PhotonView>();
            photonView.ViewID = 0; // Will be assigned by Photon
            photonView.synchronization = ViewSynchronization.ReliableAndUnreliable;
            photonView.observableSearch = ObservableSearch.Manual;
            
            // Make sure the GameObject is inactive initially
            radioObj.SetActive(false);
            
            return radioObj;
        }
        
        /// <summary>
        /// Create visual components for the radio
        /// </summary>
        private static void CreateRadioVisuals(GameObject radioObj)
        {
            // Add MeshRenderer and MeshFilter
            MeshRenderer renderer = radioObj.AddComponent<MeshRenderer>();
            MeshFilter filter = radioObj.AddComponent<MeshFilter>();
            
            // Create radio mesh (simple box for now - will be replaced with asset bundle)
            filter.mesh = CreateRadioMesh();
            
            // Create material for radio
            Material radioMaterial = new Material(Shader.Find("Standard"));
            radioMaterial.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray
            radioMaterial.metallic = 0.8f;
            radioMaterial.smoothness = 0.6f;
            renderer.material = radioMaterial;
            
            // Add some simple detail objects
            CreateRadioDetails(radioObj);
        }
        
        /// <summary>
        /// Create simple radio mesh (placeholder until asset bundle)
        /// </summary>
        private static Mesh CreateRadioMesh()
        {
            // Create a simple box mesh for the radio
            var mesh = new Mesh();
            
            // Define vertices for a radio-shaped box
            Vector3[] vertices = new Vector3[]
            {
                // Bottom face
                new Vector3(-0.3f, 0f, -0.2f),
                new Vector3(0.3f, 0f, -0.2f),
                new Vector3(0.3f, 0f, 0.2f),
                new Vector3(-0.3f, 0f, 0.2f),
                // Top face
                new Vector3(-0.3f, 0.2f, -0.2f),
                new Vector3(0.3f, 0.2f, -0.2f),
                new Vector3(0.3f, 0.2f, 0.2f),
                new Vector3(-0.3f, 0.2f, 0.2f),
            };
            
            // Define triangles
            int[] triangles = new int[]
            {
                // Bottom
                0, 1, 2, 0, 2, 3,
                // Top
                4, 6, 5, 4, 7, 6,
                // Front
                0, 4, 5, 0, 5, 1,
                // Back
                2, 6, 7, 2, 7, 3,
                // Left
                0, 3, 7, 0, 7, 4,
                // Right
                1, 5, 6, 1, 6, 2
            };
            
            // Define UVs
            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        /// <summary>
        /// Add some basic detail objects to make the radio more recognizable
        /// </summary>
        private static void CreateRadioDetails(GameObject radioObj)
        {
            // Create antenna
            GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antenna.transform.SetParent(radioObj.transform);
            antenna.transform.localPosition = new Vector3(0.2f, 0.2f, 0.15f);
            antenna.transform.localScale = new Vector3(0.02f, 0.3f, 0.02f);
            antenna.name = "Radio_Antenna";
            
            // Create speaker (visual only)
            GameObject speaker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            speaker.transform.SetParent(radioObj.transform);
            speaker.transform.localPosition = new Vector3(0f, 0.15f, 0.201f);
            speaker.transform.localScale = new Vector3(0.15f, 0.01f, 0.15f);
            speaker.name = "Radio_Speaker";
            
            // Make sure these don't interfere with physics
            Object.Destroy(antenna.GetComponent<Collider>());
            Object.Destroy(speaker.GetComponent<Collider>());
        }
            filter.mesh = CreateRadioMesh();
            
            // Create radio material
            Material radioMaterial = new Material(Shader.Find("Standard"));
            radioMaterial.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            radioMaterial.SetFloat("_Metallic", 0.7f);
            radioMaterial.SetFloat("_Smoothness", 0.4f);
            renderer.material = radioMaterial;
            
            // Add antenna (child object)
            CreateAntenna(radioObj);
            
            // Add LED indicators
            CreateLEDIndicators(radioObj);
        }
        
        private static void CreateAntenna(GameObject parent)
        {
            GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antenna.name = "Antenna";
            antenna.transform.SetParent(parent.transform);
            antenna.transform.localPosition = new Vector3(0.3f, 0.3f, -0.2f);
            antenna.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            antenna.transform.localScale = new Vector3(0.02f, 0.4f, 0.02f);
            
            // Remove collider from antenna
            Object.DestroyImmediate(antenna.GetComponent<Collider>());
            
            // Antenna material
            Material antennaMaterial = new Material(Shader.Find("Standard"));
            antennaMaterial.color = Color.gray;
            antennaMaterial.SetFloat("_Metallic", 0.9f);
            antenna.GetComponent<MeshRenderer>().material = antennaMaterial;
        }
        
        private static void CreateLEDIndicators(GameObject parent)
        {
            // Power LED
            GameObject powerLED = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            powerLED.name = "PowerLED";
            powerLED.transform.SetParent(parent.transform);
            powerLED.transform.localPosition = new Vector3(-0.3f, 0.1f, 0.4f);
            powerLED.transform.localScale = Vector3.one * 0.05f;
            
            // Remove collider
            Object.DestroyImmediate(powerLED.GetComponent<Collider>());
            
            // LED material (will glow when playing)
            Material ledMaterial = new Material(Shader.Find("Standard"));
            ledMaterial.color = Color.red;
            ledMaterial.SetFloat("_Metallic", 0f);
            ledMaterial.SetFloat("_Smoothness", 1f);
            ledMaterial.EnableKeyword("_EMISSION");
            powerLED.GetComponent<MeshRenderer>().material = ledMaterial;
        }
        
        private static void AddPhysicsComponents(GameObject radioObj)
        {
            // Add BoxCollider for interaction
            BoxCollider collider = radioObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.8f, 0.4f, 0.6f);
            collider.center = new Vector3(0f, 0.2f, 0f);
            
            // Rigidbody will be added by the Item component automatically
        }
        
        private static void SetupItemInteractions(Item itemComponent, ClimbTunesRadio radioComponent)
        {
            // Primary interaction: Turn on/off or play/pause
            itemComponent.OnPrimaryFinishedCast += () =>
            {
                if (radioComponent.isPlaying)
                {
                    radioComponent.Pause();
                }
                else if (radioComponent.isPaused)
                {
                    radioComponent.Resume();
                }
                else
                {
                    // Play default playlist
                    string defaultUrl = ClimbTunesPlugin.Instance.defaultPlaylist.Value;
                    if (!string.IsNullOrEmpty(defaultUrl))
                    {
                        radioComponent.PlayURL(defaultUrl);
                    }
                }
            };
            
            // Secondary interaction: Open radio interface
            itemComponent.OnSecondaryFinishedCast += () =>
            {
                ClimbTunesPlugin.Instance.OpenRadioUI();
            };
        }
        
        private static Mesh CreateRadioMesh()
        {
            // Create a simple radio-shaped mesh
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = temp.GetComponent<MeshFilter>().mesh;
            Object.DestroyImmediate(temp);
            
            // Scale vertices to make it more radio-like
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(
                    vertices[i].x * 0.4f,
                    vertices[i].y * 0.2f,
                    vertices[i].z * 0.3f
                );
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        /// <summary>
        /// Spawn a radio item in the world
        /// This uses the proper game systems for item spawning
        /// </summary>
        public static void SpawnRadioItem(Vector3 position)
        {
            if (RadioItemContent?.Item != null)
            {
                // Use the game's item spawning system
                ItemDatabase.Add(RadioItemContent.Item, position);
            }
        }
    }
}

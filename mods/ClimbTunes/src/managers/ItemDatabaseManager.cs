using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;
using Zorro.Core;

namespace ClimbTunes.Manager
{
    public class ItemDatabaseManager
    {
        private static bool isItemRegistered = false;
        private const ushort RADIO_ITEM_ID = 62857;

        public static void RegisterRadioItem(GameObject radioPrefab)
        {
            if (isItemRegistered) return;

            try
            {
                // Get the ItemDatabase instance
                ItemDatabase itemDatabase = SingletonAsset<ItemDatabase>.Instance;
                if (itemDatabase == null)
                {
                    ClimbTunes.ModLogger.LogError("ItemDatabase instance is null");
                    return;
                }

                // Get the Item component from the prefab
                Item radioItem = radioPrefab.GetComponent<Item>();
                if (radioItem == null)
                {
                    ClimbTunes.ModLogger.LogError("Radio prefab is missing Item component");
                    return;
                }

                // Use the fixed itemID
                ushort itemID = RADIO_ITEM_ID;

                // Check for collisions and resolve them
                if (itemDatabase.itemLookup.ContainsKey(itemID))
                {
                    ClimbTunes.ModLogger.LogWarning($"Collision on hash itemID '{itemID}' for Radio item");
                    if (TryResolveCollision(itemDatabase, ref itemID))
                    {
                        ClimbTunes.ModLogger.LogWarning($"itemID changed to '{itemID}' for Radio item");
                    }
                    else
                    {
                        ClimbTunes.ModLogger.LogError($"Could not resolve collision on itemID '{itemID}' for Radio item");
                        return;
                    }
                }

                // Set the itemID on the Item component
                radioItem.itemID = itemID;

                // Fix shader issues (replace dummy shader with game's shader)
                FixShaders(radioItem);

                // Add item to database
                itemDatabase.Objects.Add(radioItem);
                itemDatabase.itemLookup.Add(itemID, radioItem);

                isItemRegistered = true;
                ClimbTunes.ModLogger.LogInfo($"Radio item registered successfully with ID: {itemID}");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error registering radio item: {ex.Message}");
            }
        }

        private static bool TryResolveCollision(ItemDatabase itemDatabase, ref ushort id)
        {
            // This should overflow and loop back to check every entry in the dictionary.
            for (ushort i = (ushort)(id + 1); i != id; i++)
            {
                if (!itemDatabase.itemLookup.ContainsKey(i))
                {
                    id = i;
                    return true;
                }
            }
            return false;
        }

        private static void FixShaders(Item item)
        {
            try
            {
                Shader peakShader = Shader.Find("W/Peak_Standard");
                if (peakShader == null)
                {
                    ClimbTunes.ModLogger.LogWarning("Peak_Standard shader not found");
                    return;
                }

                foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.material.shader.name.Contains("Peak_Standard"))
                    {
                        // Replace dummy shader with the real one
                        renderer.material.shader = peakShader;
                    }
                }

                // Fix smoke particle system if it exists
                var particleSystem = item.gameObject.GetComponentInChildren<ParticleSystem>();
                if (particleSystem != null)
                {
                    var smokeMaterial = Resources.FindObjectsOfTypeAll<Material>()
                        .FirstOrDefault(x => x.name == "Smoke");
                    
                    if (smokeMaterial != null)
                    {
                        particleSystem.GetComponent<ParticleSystemRenderer>().material = smokeMaterial;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogWarning($"Error fixing shaders: {ex.Message}");
            }
        }

        public static ushort GetRadioItemID()
        {
            return RADIO_ITEM_ID; // Return the fixed ID directly
        }
    }
}
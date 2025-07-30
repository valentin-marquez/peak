using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;

namespace ClimbTunes.Manager
{
    public static class FontManager
    {
        private static TMP_FontAsset _gameFont;
        private static readonly string[] FONT_FAMILY_NAMES = { "DarumaDropOne", "Daruma Drop One" };
        private const string TMP_SHADER_NAME = "TextMeshPro/Distance Field";
        
        #region Public API
        
        /// <summary>
        /// Gets the game's Daruma Drop One font asset
        /// </summary>
        public static TMP_FontAsset GameFont => _gameFont ??= FindGameFont();
        
        /// <summary>
        /// Checks if the game font is available
        /// </summary>
        public static bool IsGameFontAvailable => GameFont != null;
        
        /// <summary>
        /// Applies the game font to all TextMeshProUGUI components in a GameObject hierarchy
        /// </summary>
        public static void ApplyGameFontToPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                ClimbTunes.ModLogger.LogWarning("Cannot apply font to null prefab");
                return;
            }

            if (!IsGameFontAvailable)
            {
                ClimbTunes.ModLogger.LogError("Game font not available, skipping font replacement");
                return;
            }

            ClimbTunes.ModLogger.LogInfo($"Applying game font to prefab: {prefab.name}");
            
            PrepareGameFont();
            var textComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            int updatedCount = ApplyFontToComponents(textComponents);
            
            ClimbTunes.ModLogger.LogInfo($"Font replacement complete. Updated {updatedCount}/{textComponents.Length} text components");
        }

        /// <summary>
        /// Waits for the game font to be available, then applies it to the prefab
        /// </summary>
        public static IEnumerator WaitAndApplyGameFont(GameObject prefab)
        {
            yield return new WaitUntil(() => IsGameFontAvailable);
            ApplyGameFontToPrefab(prefab);
        }

        /// <summary>
        /// Lists all available TMP fonts for debugging
        /// </summary>
        public static void LogAvailableFonts()
        {
            try
            {
                var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                ClimbTunes.ModLogger.LogInfo($"=== Available TMP Fonts ({fonts.Length}) ===");
                
                foreach (var font in fonts.Where(f => f != null))
                {
                    ClimbTunes.ModLogger.LogInfo($"Font: {font.name} | Family: {font.faceInfo.familyName} | Style: {font.faceInfo.styleName}");
                }
                
                ClimbTunes.ModLogger.LogInfo("=== End of fonts list ===");
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error listing fonts: {ex.Message}");
            }
        }
        
        #endregion

        #region Private Implementation
        
        /// <summary>
        /// Finds the game's Daruma font using multiple search strategies
        /// </summary>
        private static TMP_FontAsset FindGameFont()
        {
            var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            
            // Strategy 1: Exact family name match (case-insensitive)
            foreach (var familyName in FONT_FAMILY_NAMES)
            {
                var font = allFonts.FirstOrDefault(f => 
                    string.Equals(f.faceInfo.familyName, familyName, System.StringComparison.OrdinalIgnoreCase));
                
                if (font != null)
                {
                    ClimbTunes.ModLogger.LogInfo($"Game font found by family name '{familyName}': {font.name}");
                    return font;
                }
            }
            
            // Strategy 2: Font name contains "Daruma" (fallback)
            var fallbackFont = allFonts.FirstOrDefault(f => 
                f.name.IndexOf("Daruma", System.StringComparison.OrdinalIgnoreCase) >= 0);
            
            if (fallbackFont != null)
            {
                ClimbTunes.ModLogger.LogInfo($"Game font found by name fallback: {fallbackFont.name}");
                return fallbackFont;
            }
            
            ClimbTunes.ModLogger.LogError("No Daruma font found. Available fonts will be logged...");
            LogAvailableFonts();
            return null;
        }
        
        /// <summary>
        /// Prepares the game font by ensuring its material and texture are properly configured
        /// </summary>
        private static void PrepareGameFont()
        {
            if (GameFont?.material == null)
            {
                ClimbTunes.ModLogger.LogWarning("Game font material is null");
                return;
            }

            // Ensure atlas texture is linked
            if (GameFont.material.mainTexture == null && GameFont.atlasTexture != null)
            {
                GameFont.material.mainTexture = GameFont.atlasTexture;
                ClimbTunes.ModLogger.LogInfo("Atlas texture re-linked to font material");
            }

            // Verify material state
            if (GameFont.material.mainTexture != null)
            {
                ClimbTunes.ModLogger.LogInfo($"Font material ready: {GameFont.material.name}");
            }
            else
            {
                ClimbTunes.ModLogger.LogWarning("Font material has no texture - UI may show as magenta");
            }
        }
        
        /// <summary>
        /// Applies the game font to an array of TextMeshProUGUI components
        /// </summary>
        private static int ApplyFontToComponents(TextMeshProUGUI[] components)
        {
            int updatedCount = 0;
            
            foreach (var textComponent in components.Where(c => c != null))
            {
                if (ApplyFontToComponent(textComponent))
                {
                    updatedCount++;
                }
            }
            
            return updatedCount;
        }
        
        /// <summary>
        /// Applies the game font to a single TextMeshProUGUI component
        /// </summary>
        private static bool ApplyFontToComponent(TextMeshProUGUI textComponent)
        {
            try
            {
                // Store original properties
                var originalProperties = new TextProperties(textComponent);
                
                ClimbTunes.ModLogger.LogInfo($"Updating font for: {GetGameObjectPath(textComponent.gameObject)}");
                ClimbTunes.ModLogger.LogInfo($"  Original font: {textComponent.font?.name ?? "null"} | Text: '{originalProperties.Text}'");
                
                // Apply font and fix material
                textComponent.font = GameFont;
                FixComponentMaterial(textComponent);
                
                // Restore original properties
                originalProperties.RestoreTo(textComponent);
                
                ClimbTunes.ModLogger.LogInfo($"  Font updated successfully to: {GameFont.name}");
                return true;
            }
            catch (System.Exception ex)
            {
                ClimbTunes.ModLogger.LogError($"Error updating font for {textComponent.name}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Fixes the material shader for a TextMeshProUGUI component
        /// </summary>
        private static void FixComponentMaterial(TextMeshProUGUI textComponent)
        {
            if (textComponent.font?.material == null) return;

            var correctShader = Shader.Find(TMP_SHADER_NAME);
            if (correctShader != null && textComponent.font.material.shader != correctShader)
            {
                textComponent.font.material.shader = correctShader;
                ClimbTunes.ModLogger.LogInfo($"Fixed material shader for {textComponent.name}");
            }
        }
        
        /// <summary>
        /// Gets the full hierarchy path of a GameObject
        /// </summary>
        private static string GetGameObjectPath(GameObject obj)
        {
            var path = obj.name;
            var parent = obj.transform.parent;
            
            while (parent != null)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }
            
            return path;
        }
        
        #endregion

        #region Helper Classes
        
        /// <summary>
        /// Stores and restores TextMeshProUGUI properties
        /// </summary>
        private class TextProperties
        {
            public string Text { get; }
            public float FontSize { get; }
            public Color Color { get; }
            public TextAlignmentOptions Alignment { get; }
            public FontStyles FontStyle { get; }

            public TextProperties(TextMeshProUGUI textComponent)
            {
                Text = textComponent.text;
                FontSize = textComponent.fontSize;
                Color = textComponent.color;
                Alignment = textComponent.alignment;
                FontStyle = textComponent.fontStyle;
            }

            public void RestoreTo(TextMeshProUGUI textComponent)
            {
                textComponent.text = Text;
                textComponent.fontSize = FontSize;
                textComponent.color = Color;
                textComponent.alignment = Alignment;
                textComponent.fontStyle = FontStyle;
            }
        }
        
        #endregion
    }
}
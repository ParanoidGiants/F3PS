using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class TextureAtlasGenerator : EditorWindow
{
    // Atlas settings
    int atlasWidth = 1024;
    int atlasHeight = 1024;
    int gridCols = 4; // Number of columns in the atlas grid
    int gridRows = 4; // Number of rows in the atlas grid

    // Each entry holds a texture along with its grid coordinates.
    // Now gridX represents the row (starting at 0 from the bottom)
    // and gridY represents the column (starting at 0 from the left)
    [System.Serializable]
    public class TextureEntry
    {
        public Texture2D texture;
        public int gridX; // Row index
        public int gridY; // Column index
    }

    // List to store user-added textures and their grid positions.
    List<TextureEntry> textures = new List<TextureEntry>();

    // Add a menu item to open the window.
    [MenuItem("Tools/Texture Atlas Generator")]
    public static void ShowWindow()
    {
        GetWindow<TextureAtlasGenerator>("Texture Atlas Generator");
    }

    void OnGUI()
    {
        // Atlas resolution and grid settings.
        GUILayout.Label("Atlas Settings", EditorStyles.boldLabel);
        atlasWidth = EditorGUILayout.IntField("Atlas Width", atlasWidth);
        atlasHeight = EditorGUILayout.IntField("Atlas Height", atlasHeight);
        gridCols = EditorGUILayout.IntField("Grid Columns", gridCols);
        gridRows = EditorGUILayout.IntField("Grid Rows", gridRows);

        // Display a note about grid coordinate origin.
        EditorGUILayout.HelpBox("Note: The grid's (0, 0) is at the bottom-left. 'Row' (gridX) increases upward and 'Column' (gridY) increases to the right.", MessageType.Info);

        GUILayout.Space(10);
        GUILayout.Label("Texture Entries", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Selected Textures"))
        {
            // Loop over currently selected objects in the Project window.
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D)
                {
                    TextureEntry entry = new TextureEntry();
                    entry.texture = (Texture2D)obj;
                    // Assign a default grid position.
                    int index = textures.Count;
                    // Here gridX (row) gets the quotient and gridY (column) gets the remainder.
                    entry.gridX = index / gridCols;
                    entry.gridY = index % gridCols;
                    textures.Add(entry);
                }
            }
        }

        // Display each texture entry with fields to modify texture and grid positions.
        for (int i = 0; i < textures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            textures[i].texture = (Texture2D)EditorGUILayout.ObjectField(textures[i].texture, typeof(Texture2D), false);
            textures[i].gridX = EditorGUILayout.IntField("Row", textures[i].gridX);
            textures[i].gridY = EditorGUILayout.IntField("Column", textures[i].gridY);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                textures.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        // Generate atlas button.
        if (GUILayout.Button("Generate Atlas"))
        {
            GenerateAtlas();
        }
    }

    /// <summary>
    /// Generates the texture atlas by creating a new Texture2D of the specified resolution,
    /// then places each texture into its grid cell based on the defined grid rows and columns.
    /// </summary>
    void GenerateAtlas()
    {
        // Calculate the resolution for each grid cell.
        int cellWidth = atlasWidth / gridCols;
        int cellHeight = atlasHeight / gridRows;

        // Create a new texture for the atlas.
        Texture2D atlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);

        // Fill the atlas with transparent pixels.
        Color[] fillColorArray = atlas.GetPixels();
        for (int i = 0; i < fillColorArray.Length; i++)
        {
            fillColorArray[i] = Color.clear;
        }
        atlas.SetPixels(fillColorArray);

        // Process each texture entry.
        foreach (TextureEntry entry in textures)
        {
            if (entry.texture == null)
                continue;

            // Scale the texture to the size of one grid cell.
            Texture2D scaledTex = ScaleTexture(entry.texture, cellWidth, cellHeight);

            // Determine the starting pixel in the atlas.
            // Now, gridX (row index) defines the vertical offset and gridY (column index) defines the horizontal offset.
            int startX = entry.gridY * cellWidth;
            int startY = entry.gridX * cellHeight;

            // Copy the scaled texture's pixels into the atlas.
            atlas.SetPixels(startX, startY, cellWidth, cellHeight, scaledTex.GetPixels());
        }
        atlas.Apply();

        // Open a save file panel for the user to specify where to save the atlas PNG.
        string path = EditorUtility.SaveFilePanel("Save Texture Atlas", Application.dataPath, "TextureAtlas", "png");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, atlas.EncodeToPNG());
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Atlas Generated", "Texture atlas saved successfully!", "OK");
        }
    }

    /// <summary>
    /// Scales the provided texture to the target width and height using bilinear interpolation.
    /// </summary>
    /// <param name="source">Source texture</param>
    /// <param name="targetWidth">Desired width</param>
    /// <param name="targetHeight">Desired height</param>
    /// <returns>A new Texture2D with the scaled image.</returns>
    Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        Color[] resultColors = new Color[targetWidth * targetHeight];

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float u = (x + 0.5f) / targetWidth;
                float v = (y + 0.5f) / targetHeight;
                resultColors[y * targetWidth + x] = source.GetPixelBilinear(u, v);
            }
        }
        result.SetPixels(resultColors);
        result.Apply();
        return result;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DebugTextureDrawer : MonoBehaviour
{
    public RadiiusSensor sensor;
    public RawImage displayImage; // Assign a UI RawImage in the inspector
    private Texture2D debugTexture;
    private int textureSize = 128; // Size of the debug texture

    void Start()
    {
        // Initialize the texture
        debugTexture = new Texture2D(textureSize, textureSize);
        displayImage.texture = debugTexture;
        debugTexture.filterMode = FilterMode.Point;

        ClearTexture();
    }

    void Update()
    {
        DrawDebugInfo(sensor.DetectObjects());
    }

    void ClearTexture()
    {
        Color testColor = Color.black; // Use a test color
        for (int x = 0; x < debugTexture.width; x++)
        {
            for (int y = 0; y < debugTexture.height; y++)
            {
                debugTexture.SetPixel(x, y, testColor);
            }
        }
        debugTexture.Apply();
    }

    void DrawDebugInfo(List<RadiiusSensor.ObjectData> objects)
    {
        ClearTexture();

        //Debug.Log("Objects detected: " + objects.Count);
        foreach (var obj in objects)
        {
            // Convert world position to texture position

            //Debug.Log("norm.x" + obj.normalizedPosition.x);
            //Debug.Log("norm.y" + obj.normalizedPosition.y);
            Vector2 texPos = new Vector2(obj.normalizedPosition.x * (textureSize - 1), obj.normalizedPosition.y * (textureSize - 1));

            //Debug.Log("texPos: " + texPos);
            Color drawColor = GetColorBasedOnTag(obj.oneHotTag);

            // Draw a larger dot for each object
            DrawLargerDot((int)texPos.x, (int)texPos.y, drawColor);
        }

        debugTexture.Apply();
    }

    void DrawLargerDot(int x, int y, Color color)
    {
        int size = 1; // Size of the dot, adjust as needed

        for (int i = -size; i <= size; i++)
        {
            for (int j = -size; j <= size; j++)
            {
                int drawX = Mathf.Clamp(x + i, 0, textureSize - 1);
                int drawY = Mathf.Clamp(y + j, 0, textureSize - 1);
                debugTexture.SetPixel(drawX, drawY, color);
            }
        }
    }

    Color GetColorBasedOnTag(int[] oneHotTag)
    {
        // Determine the color based on the object's tag
        // Example: return Color.red for enemies, Color.green for allies, etc.
        // This depends on the order of tags in your one-hot encoding
        if (oneHotTag[0] == 1) return Color.red; // Assuming first tag is "Enemy"
        if (oneHotTag[1] == 1) return Color.green; // "Player"
        if (oneHotTag[2] == 1) return Color.blue; // "Player"
        // ... Add more conditions based on your tags

        return Color.white; // Default color if no tag matches
    }
}
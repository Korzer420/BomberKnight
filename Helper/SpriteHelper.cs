using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BomberKnight.Helper;

public static class SpriteHelper
{
    private static Dictionary<string, Sprite> _cachedSprites = new();

    public static Sprite CreateSprite(string spriteName) => CreateSprite(spriteName, ".png");

    /// <summary>
    /// Creates a sprite from the given image path. Starts in this Resource folder.
    /// </summary>
    public static Sprite CreateSprite(string spriteName, string extension)
    {
        if (!_cachedSprites.ContainsKey(spriteName))
        {
            // Don't ask...
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream("BomberKnight.Resources." + spriteName + extension);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            byte[] imageData = ms.ToArray();
            Texture2D tex = new(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, imageData, true);
            tex.filterMode = FilterMode.Bilinear;
            _cachedSprites.Add(spriteName, Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f)));
        }
        return _cachedSprites[spriteName];
    }
}
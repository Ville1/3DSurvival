using System.Collections.Generic;
using UnityEngine;

public class SpriteManager
{
    public enum SpriteType { Block, Item, UI };
    private static SpriteManager instance;

    private Dictionary<SpriteType, string> prefixes;
    private Dictionary<string, Sprite> sprites;
    private bool suppress_error_logging;

    private SpriteManager()
    {
        suppress_error_logging = false;
        prefixes = new Dictionary<SpriteType, string>();
        sprites = new Dictionary<string, Sprite>();

        prefixes.Add(SpriteType.Block, "block");
        prefixes.Add(SpriteType.Item, "item");
        prefixes.Add(SpriteType.UI, "ui");

        CustomLogger.Instance.Debug("Loading sprites...");
        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/blocks")) {
            sprites.Add(prefixes[SpriteType.Block] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Block sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/items")) {
            sprites.Add(prefixes[SpriteType.Item] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Item sprite loaded: " + texture.name);
        }
        
        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/ui")) {
            sprites.Add(prefixes[SpriteType.UI] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("UI sprite loaded: " + texture.name);
        }
        CustomLogger.Instance.Debug("All sprites loaded");
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static SpriteManager Instance
    {
        get {
            if (instance == null) {
                instance = new SpriteManager();
            }
            return instance;
        }
    }

    /// <summary>
    /// Get sprite by name and type
    /// </summary>
    /// <param name="sprite_name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Sprite Get_Sprite(string sprite_name, SpriteType type)
    {
        if (sprites.ContainsKey(prefixes[type] + "_" + sprite_name)) {
            return sprites[prefixes[type] + "_" + sprite_name];
        }
        if (!suppress_error_logging) {
            CustomLogger.Instance.Warning("Sprite " + prefixes[type] + "_" + sprite_name + " does not exist!");
        }
        return null;
    }
}

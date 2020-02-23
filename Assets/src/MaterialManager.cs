using System.Collections.Generic;
using UnityEngine;

public class MaterialManager {
    public enum MaterialType { Block };
    private static MaterialManager instance;

    private Dictionary<MaterialType, string> prefixes;
    private Dictionary<string, Material> materials;
    private bool suppress_error_logging;

    private MaterialManager()
    {
        suppress_error_logging = false;
        prefixes = new Dictionary<MaterialType, string>();
        materials = new Dictionary<string, Material>();

        prefixes.Add(MaterialType.Block, "block");

        CustomLogger.Instance.Debug("Loading materials...");
        foreach (Material material in Resources.LoadAll<Material>("materials/blocks")) {
            materials.Add(prefixes[MaterialType.Block] + "_" + material.name, material);
            CustomLogger.Instance.Debug("Block material loaded: " + material.name);
        }
        CustomLogger.Instance.Debug("All materials loaded");
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static MaterialManager Instance
    {
        get {
            if (instance == null) {
                instance = new MaterialManager();
            }
            return instance;
        }
    }
    
    public Material Get(string material_name, MaterialType type)
    {
        if (materials.ContainsKey(prefixes[type] + "_" + material_name)) {
            return materials[prefixes[type] + "_" + material_name];
        }
        if (!suppress_error_logging) {
            CustomLogger.Instance.Warning("Material " + prefixes[type] + "_" + material_name + " does not exist!");
        }
        return null;
    }
}

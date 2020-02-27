using System.Collections.Generic;
using UnityEngine;

public class ModelManager {
    private readonly string[] MODELS = new string[7] { "grass", "short_grass", "medium_grass", "tall_grass", "stones", "sticks", "flint" };

    private Dictionary<string, GameObject> models;
    
    private static ModelManager instance;

    private ModelManager()
    {
        models = new Dictionary<string, GameObject>();
        CustomLogger.Instance.Debug("Loading 3D models...");
        foreach (string model_name in MODELS) {
            GameObject model = Resources.Load<GameObject>(string.Format("models/{0}/{1}", model_name, model_name));
            if(model == null) {
                CustomLogger.Instance.Error(string.Format("Failed to load model: {0}", model_name));
            } else {
                models.Add(model_name, model);
                CustomLogger.Instance.Debug(string.Format("Model loaded: {0}", model_name));
            }
        }
        CustomLogger.Instance.Debug("All 3D models loaded");
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static ModelManager Instance
    {
        get {
            if (instance == null) {
                instance = new ModelManager();
            }
            return instance;
        }
    }

    public GameObject Get(string name)
    {
        if (!models.ContainsKey(name)) {
            CustomLogger.Instance.Error(string.Format("Model not found: {0}", name));
            return null;
        }
        return models[name];
    }
}

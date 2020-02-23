﻿using System.Reflection;
using UnityEngine;

public class ModelManager : MonoBehaviour
{
    public static ModelManager Instance { get; private set; }

    public GameObject Grass;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Warning(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {

    }

    public GameObject Get(string name)
    {
        foreach (FieldInfo info in GetType().GetFields()) {
            if (info.Name == name && info.GetType() == typeof(GameObject)) {
                return (GameObject)info.GetValue(this);
            }
        }
        return null;
    }
}

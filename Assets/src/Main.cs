﻿using UnityEngine;

public class Main : MonoBehaviour
{
    public static readonly float VERSION = 0.1f;

    public static Main Instance;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        Map.Instance.Update(Time.deltaTime);
    }

    /// <summary>
    /// Exits the game
    /// </summary>
    public static void Quit()
    {
        Application.Quit();
    }
}

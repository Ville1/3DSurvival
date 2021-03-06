﻿using UnityEngine;

public class MasterUIManager : MonoBehaviour
{
    public static MasterUIManager Instance { get; private set; }

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

    }

    /// <summary>
    /// Closes all menus and UI elements
    /// </summary>
    public void Close_All()
    {
        ConsoleManager.Instance.Close_Console();
        MainMenuManager.Instance.Active = false;
    }

    public void Close_Others(string type_name)
    {
        if(MainMenuManager.Instance != null && typeof(MainMenuManager).Name != type_name) {
            MainMenuManager.Instance.Active = false;
        }
        if (InventoryGUIManager.Instance != null && typeof(InventoryGUIManager).Name != type_name) {
            InventoryGUIManager.Instance.Active = false;
        }
        if (CraftingMenuManager.Instance != null && typeof(CraftingMenuManager).Name != type_name) {
            CraftingMenuManager.Instance.Active = false;
        }
        if (BuildMenuManager.Instance != null && typeof(BuildMenuManager).Name != type_name) {
            BuildMenuManager.Instance.Active = false;
        }
        if (SaveGUIManager.Instance != null && typeof(SaveGUIManager).Name != type_name) {
            SaveGUIManager.Instance.Active = false;
        }
        if (ConfirmationDialogManager.Instance != null && typeof(ConfirmationDialogManager).Name != type_name) {
            ConfirmationDialogManager.Instance.Active = false;
        }
        if (LoadGUIManager.Instance != null && typeof(LoadGUIManager).Name != type_name) {
            LoadGUIManager.Instance.Active = false;
        }
        if (NewGameGUIManager.Instance != null && typeof(NewGameGUIManager).Name != type_name) {
            NewGameGUIManager.Instance.Active = false;
        }
    }

    public bool Window_Is_Open
    {
        get {
            return
                (MainMenuManager.Instance != null && MainMenuManager.Instance.Active) ||
                (InventoryGUIManager.Instance != null && InventoryGUIManager.Instance.Active) ||
                (CraftingMenuManager.Instance != null && CraftingMenuManager.Instance.Active) ||
                (BuildMenuManager.Instance != null && BuildMenuManager.Instance.Active) ||
                (SaveGUIManager.Instance != null && SaveGUIManager.Instance.Active) ||
                (ConfirmationDialogManager.Instance != null && ConfirmationDialogManager.Instance.Active) ||
                (LoadGUIManager.Instance != null && LoadGUIManager.Instance.Active) ||
                (NewGameGUIManager.Instance != null && NewGameGUIManager.Instance.Active);
        }
    }

    public bool Intercept_Keyboard_Input
    {
        get {
            return ConsoleManager.Instance.Is_Open() || SaveGUIManager.Instance.Active;
        }
    }

    public void Read_Keyboard_Input()
    {
        if (ConsoleManager.Instance.Is_Open()) {
            if (Input.GetButtonDown("Submit")) {
                ConsoleManager.Instance.Run_Command();
            }
            if (Input.GetButtonDown("Console scroll down")) {
                ConsoleManager.Instance.Scroll_Down();
            }
            if (Input.GetButtonDown("Console scroll up")) {
                ConsoleManager.Instance.Scroll_Up();
            }
            if (Input.GetButtonDown("Auto complete")) {
                ConsoleManager.Instance.Auto_Complete();
            }
            if (Input.GetButtonDown("Console history up")) {
                ConsoleManager.Instance.Command_History_Up();
            }
            if (Input.GetButtonDown("Console history down")) {
                ConsoleManager.Instance.Command_History_Down();
            }
        }
    }
}

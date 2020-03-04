using System;
using UnityEngine;
using UnityEngine.UI;

public class NewGameGUIManager : MonoBehaviour {
    public static NewGameGUIManager Instance { get; private set; }

    private static readonly int DEFAULT_HEIGHT = 15;
    private static readonly int DEFAULT_SIZE = 7;

    public GameObject Panel;

    public InputField Seed_Input;
    public Toggle SI_Toggle;
    public Toggle Infinite_Generation_Toggle;
    public GameObject Dimensions_Container;
    public InputField Y_Input;
    public InputField X_Input;
    public InputField Z_Input;
    public Button OK_Button;

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
        Panel.SetActive(false);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!Active) {
            return;
        }
        OK_Button.interactable = Validate();
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if ((value && Panel.activeSelf) || (!value && !Panel.activeSelf)) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_Others(GetType().Name);
                Initialize();
            }
            Panel.SetActive(value);
        }
    }

    public void Toggle_Infinite_Generation()
    {
        Dimensions_Container.SetActive(!Infinite_Generation_Toggle.isOn);
    }

    public void Start_Game()
    {
        if (!Validate()) {
            return;
        }
        int seed = int.Parse(Seed_Input.text);
        int y = int.Parse(Y_Input.text);
        int x = Infinite_Generation_Toggle.isOn ? DEFAULT_SIZE : int.Parse(X_Input.text);
        int z = Infinite_Generation_Toggle.isOn ? DEFAULT_SIZE : int.Parse(Z_Input.text);
        Active = false;
        RNG.Instance.Set_Seed(seed);
        Map.Instance.Generate_New(y, x, z, !Infinite_Generation_Toggle.isOn, true, SI_Toggle.isOn);
    }

    private void Initialize()
    {
        Seed_Input.text = Guid.NewGuid().GetHashCode().ToString();
        SI_Toggle.isOn = false;
        Infinite_Generation_Toggle.isOn = false;
        Dimensions_Container.SetActive(true);
        Y_Input.text = DEFAULT_HEIGHT.ToString();
        X_Input.text = DEFAULT_SIZE.ToString();
        Z_Input.text = DEFAULT_SIZE.ToString();
    }

    private bool Validate()
    {
        int i;
        if (!int.TryParse(Seed_Input.text, out i)) {
            return false;
        }
        if (!int.TryParse(Y_Input.text, out i)) {
            return false;
        }
        if (i <= 10) {
            return false;
        }
        if (!Infinite_Generation_Toggle.isOn) {
            if (!int.TryParse(X_Input.text, out i)) {
                return false;
            }
            if (i <= 0) {
                return false;
            }
            if (!int.TryParse(Z_Input.text, out i)) {
                return false;
            }
            if (i <= 0) {
                return false;
            }
        }
        return true;
    }
}

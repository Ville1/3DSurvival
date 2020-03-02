using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    public GameObject Panel;

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
            }
            Panel.SetActive(value);
        }
    }

    public void Toggle()
    {
        Active = !Active;
    }

    public void New_Game_Button_On_Click()
    {
        Active = false;
        //Map.Instance.Generate_New(2, 15, 2, 3, 3, true, true);
        Map.Instance.Generate_New(5, 15, 5, 7, 7, true, true, false);
        //Map.Instance.Generate_New(4, 15, 4, 5, 5, true, true);
    }

    public void Exit_Button_On_Click()
    {
        Main.Quit();
    }

    public void Save_Game_Button_On_Click()
    {
        if (!Map.Instance.Active) {
            return;
        }
        SaveGUIManager.Instance.Active = true;
    }

    public void Load_Game_Button_On_Click()
    {
        LoadGUIManager.Instance.Active = true;
    }
}

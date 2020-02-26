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

    public bool Visible
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if ((value && Panel.activeSelf) || (!value && !Panel.activeSelf)) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_All();
            }
            Panel.SetActive(value);
        }
    }

    public void Toggle()
    {
        Visible = !Visible;
    }

    public void New_Game_Button_On_Click()
    {
        Visible = false;
        //Map.Instance.Generate_New(5, 10, 5, 7, 7, true);
        Map.Instance.Generate_New(2, 10, 2, 2, 2, true);
    }

    public void Exit_Button_On_Click()
    {
        Main.Quit();
    }
}

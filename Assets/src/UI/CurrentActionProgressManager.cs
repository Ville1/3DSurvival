using UnityEngine;
using UnityEngine.UI;

public class CurrentActionProgressManager : MonoBehaviour
{
    public static CurrentActionProgressManager Instance { get; private set; }

    public GameObject Panel;
    public Text Text;
    public Slider Slider;

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
    private void Update() {
        if(Player.Current == null || MasterUIManager.Instance.Window_Is_Open || Player.Current.Current_Action_Progress < 0.0f) {
            Active = false;
        } else {
            Active = true;
            Text.text = Player.Current.Current_Action_Text;
            Slider.value = Player.Current.Current_Action_Progress;
        }
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        private set {
            if(Panel.activeSelf == value) {
                return;
            }
            Panel.SetActive(value);
        }
    }
}

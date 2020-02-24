using UnityEngine;
using UnityEngine.UI;

public class PlayerGUIManager : MonoBehaviour {
    public static PlayerGUIManager Instance { get; private set; }

    public GameObject Panel;
    public Text HP_Text;
    public Text Action_Text;

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
        Active = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!Active || Player.Current == null) {
            return;
        }
        HP_Text.text = string.Format("HP: {0} / {1}", Helper.Float_To_String(Player.Current.HP, 0), Player.Current.MAX_HP);
        Action_Text.text = Player.Current.Current_Action_Text;
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            Panel.SetActive(value);
        }
    }
}

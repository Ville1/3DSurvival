using UnityEngine;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager Instance { get; private set; }

    /// <summary>
    /// Initialization
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
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        if (Input.GetButtonDown("Console")) {
            ConsoleManager.Instance.Toggle_Console();
        }

        if (Input.GetButtonDown("Escape")) {
            MainMenuManager.Instance.Visible = true;
        }

        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2)) {
            MessageManager.Instance.Close_Message();
        }

        if (!MasterUIManager.Instance.Intercept_Keyboard_Input) {
            if(Player.Current != null) {
                //Movement
                Player.Current.Current_Movement = new Direction();
                if (Input.GetAxis("Vertical") > 0.0f) {
                    Player.Current.Current_Movement.X = Direction.Shift.Positive;
                }
                if (Input.GetAxis("Vertical") < 0.0f) {
                    Player.Current.Current_Movement.X = Direction.Shift.Negative;
                }
                if (Input.GetAxis("Horizontal") > 0.0f) {
                    Player.Current.Current_Movement.Z = Direction.Shift.Positive;
                }
                if (Input.GetAxis("Horizontal") < 0.0f) {
                    Player.Current.Current_Movement.Z = Direction.Shift.Negative;
                }
                if (Input.GetButtonDown("Jump")) {
                    Player.Current.Current_Movement.Y = Direction.Shift.Positive;
                }

                if (Input.GetButtonDown("Dismantle block") || Input.GetButtonDown("Repair block") || Input.GetButtonDown("Harvest block")) {
                    Block block = InspectorManager.Instance.Block;
                    if (block == null) {
                        MessageManager.Instance.Show_Message("Select a block");
                    } else {
                        string message;
                        if (Input.GetButtonDown("Dismantle block")) {
                            if(!Player.Current.Dismantle_Block(block, out message)) {
                                MessageManager.Instance.Show_Message(message);
                            }
                        }
                        if (Input.GetButtonDown("Repair block")) {

                        }
                        if (Input.GetButtonDown("Harvest block")) {

                        }
                    }
                }
                if (Input.GetButtonDown("Inventory")) {
                    InventoryGUIManager.Instance.Toggle();
                }
            }
        } else {
            MasterUIManager.Instance.Read_Keyboard_Input();
        }
    }
}
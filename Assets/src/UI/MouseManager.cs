using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    public GameObject Crosshair;

    private bool cursor_visibility;

    public Vector3 Hit_Normal { get; private set; }

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

        cursor_visibility = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Crosshair.SetActive(false);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        Show_Cursor = BuildMenuManager.Instance.Active || CraftingMenuManager.Instance.Active || InventoryGUIManager.Instance.Active || MainMenuManager.Instance.Active || SaveGUIManager.Instance.Active
            || ConfirmationDialogManager.Instance.Active || LoadGUIManager.Instance.Active || !Map.Instance.Active || Map.Instance.Paused;
        if (!Show_Cursor) {
            CameraManager.Instance.Rotate_Camera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        if (Input.GetMouseButtonDown(0)) {
            InspectorManager.Instance.Target = Object_Under_Cursor;
        }
    }

    public Vector3 Mouse_Position_Relative_To_Camera
    {
        get {
            Vector3 position = Input.mousePosition;
            position.z = CameraManager.Instance.Camera.transform.position.z;
            return position;
        }
    }

    private bool Show_Cursor
    {
        get {
            return cursor_visibility;
        }
        set {
            cursor_visibility = value;
            Cursor.visible = cursor_visibility;
            Cursor.lockState = cursor_visibility ? CursorLockMode.None : CursorLockMode.Locked;
            Crosshair.SetActive(!value);
        }
    }

    public MapObject Object_Under_Cursor
    {
        get {
            if (EventSystem.current.IsPointerOverGameObject()) {
                return null;
            }
            RaycastHit hit;
            if (Physics.Raycast(CameraManager.Instance.Camera.ScreenPointToRay(Input.mousePosition), out hit)) {
                string name = hit.transform.gameObject.name;
                Hit_Normal = hit.normal;
                long? block_id = Block.Parse_Id_From_GameObject_Name(name);
                if (block_id.HasValue) {
                    return Map.Instance.Get_Block(block_id.Value);
                }
                long? entity_id = Entity.Parse_Id_From_GameObject_Name(name);
                if (entity_id.HasValue) {
                    return Map.Instance.Get_Entity(entity_id.Value);
                }
            }
            return null;
        }
    }
}

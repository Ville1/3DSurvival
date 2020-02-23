using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }
    
    private bool cursor_visibility;

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
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!Show_Cursor) {
            CameraManager.Instance.Rotate_Camera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
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

    public bool Show_Cursor
    {
        get {
            return cursor_visibility;
        }
        set {
            cursor_visibility = value;
            Cursor.visible = cursor_visibility;
            Cursor.lockState = cursor_visibility ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    /*public Tile Tile_Under_Cursor
    {
        get {
            if (World.Instance.Map == null) {
                return null;
            }
            RaycastHit hit;
            if (Physics.Raycast(CameraManager.Instance.Camera.ScreenPointToRay(Input.mousePosition), out hit) && hit.transform.gameObject.name.StartsWith(Tile.GAME_OBJECT_NAME_PREFIX)) {
                string name = hit.transform.gameObject.name;
                int x, y;
                if (!int.TryParse(name.Substring(name.IndexOf('(') + 1, name.IndexOf(',') - name.IndexOf('(') - 1), out x) ||
                    !int.TryParse(name.Substring(name.IndexOf(',') + 1, name.IndexOf(')') - name.IndexOf(',') - 1), out y)) {
                    CustomLogger.Instance.Error("String parsing error");
                    return null;
                } else {
                    return World.Instance.Map.Get_Tile_At(x, y);
                }
            }
            return null;
        }
    }*/
}

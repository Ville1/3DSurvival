using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    private Vector3 last_position;

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
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        Vector3 current_position = CameraManager.Instance.Camera.ScreenToWorldPoint(Mouse_Position_Relative_To_Camera);

        last_position = CameraManager.Instance.Camera.ScreenToWorldPoint(Mouse_Position_Relative_To_Camera);
    }

    public Vector3 Mouse_Position_Relative_To_Camera
    {
        get {
            Vector3 position = Input.mousePosition;
            position.z = CameraManager.Instance.Camera.transform.position.z;
            return position;
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

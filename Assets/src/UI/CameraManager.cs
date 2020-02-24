using System.Collections.Generic;
using UnityEngine;

public class CameraManager
{
    public static readonly float SENSITIVITY = 1.5f;
    public static readonly float MIN_ROTATION = -80.0f;
    public static readonly float MAX_ROTATION = 80.0f;

    private static CameraManager instance;
    private Camera camera;
    private GameObject canvas;

    private CameraManager()
    {
        camera = Camera.main;
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    /// <returns></returns>
    public static CameraManager Instance
    {
        get {
            if (instance == null) {
                instance = new CameraManager();
            }
            return instance;
        }
    }
    
    public void Reset()
    {
        camera = Player.Current != null ? Player.Current.Camera : Camera.main;
    }

    public void Rotate_Camera(float x, float y)
    {
        if(Player.Current == null || (x == 0.0f && y == 0.0f)) {
            return;
        }
        x *= SENSITIVITY;
        y *= SENSITIVITY;
        if (x != 0.0f) {
            Player.Current.GameObject.transform.Rotate(new Vector3(0.0f, x, 0.0f));
        }
        if (y != 0.0f) {
            Player.Current.Camera.transform.Rotate(new Vector3(-y, 0.0f, 0.0f));
            if (Player.Current.Camera.transform.rotation.eulerAngles.x > MAX_ROTATION && Player.Current.Camera.transform.rotation.eulerAngles.x < 180) {
                Player.Current.Camera.transform.eulerAngles = new Vector3(MAX_ROTATION, Player.Current.Camera.transform.rotation.eulerAngles.y, Player.Current.Camera.transform.rotation.eulerAngles.z);
            }
            if (Player.Current.Camera.transform.rotation.eulerAngles.x < 360.0f + MIN_ROTATION && Player.Current.Camera.transform.rotation.eulerAngles.x > 180) {
                Player.Current.Camera.transform.eulerAngles = new Vector3(360.0f + MIN_ROTATION, Player.Current.Camera.transform.rotation.eulerAngles.y, Player.Current.Camera.transform.rotation.eulerAngles.z);
            }
        }
    }

    /// <summary>
    /// Returns world coordinate points that make screen
    /// </summary>
    /// <returns></returns>
    public List<Vector2> Get_Screen_Location()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(camera.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f)));
        points.Add(camera.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, 0.0f)));
        points.Add(camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0.0f)));
        points.Add(camera.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, 0.0f)));
        return points;
    }

    public Camera Camera
    {
        get {
            return camera;
        }
        private set {
            camera = value;
        }
    }

    private GameObject Canvas
    {
        get {
            if (canvas != null) {
                return canvas;
            }
            canvas = GameObject.Find("Canvas");
            return canvas;
        }
    }

    /// <summary>
    /// https://answers.unity.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
    /// </summary>
    /// <param name="ui_element"></param>
    /// <param name="world_go"></param>
    public void Set_UI_Element_On_World_GO(RectTransform ui_element_rect_transform, GameObject world_go)
    {
        //first you need the RectTransform component of your canvas
        RectTransform CanvasRect = Canvas.GetComponent<RectTransform>();

        //then you calculate the position of the UI element
        //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
        Vector2 ViewportPosition = CameraManager.Instance.Camera.WorldToViewportPoint(new Vector3(world_go.transform.position.x + 0.5f, world_go.transform.position.y - 0.5f, world_go.transform.position.z));
        Vector2 WorldObject_ScreenPosition = new Vector2(
        ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

        //now you can set the position of the ui element
        ui_element_rect_transform.anchoredPosition = WorldObject_ScreenPosition;
    }
}

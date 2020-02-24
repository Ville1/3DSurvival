using System.Reflection;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }

    public GameObject Block;
    public GameObject Player;
    public GameObject Item_Pile;

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

    }

    public GameObject Get(string name)
    {
        foreach (FieldInfo info in GetType().GetFields()) {
            if (info.Name == name && info.FieldType == typeof(GameObject)) {
                return (GameObject)info.GetValue(this);
            }
        }
        return null;
    }
}

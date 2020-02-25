using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingMessageManager : MonoBehaviour
{
    private static long current_id;

    public static FloatingMessageManager Instance { get; private set; }

    public GameObject Container;
    public GameObject Text_Prototype;
    public int Max_Height;
    public int Message_Time;
    public float Cooldown;

    private List<TextData> texts;
    private Vector2 starting_position;
    private List<string> queue;
    private float current_cooldown;

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
        Text_Prototype.SetActive(false);
        starting_position = new Vector2(Text_Prototype.transform.position.x, Text_Prototype.transform.position.y);
        texts = new List<TextData>();
        queue = new List<string>();
        current_cooldown = 0.0f;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        current_cooldown = Mathf.Clamp(current_cooldown - Time.deltaTime, 0.0f, Cooldown);
        if(current_cooldown == 0.0f && queue.Count != 0) {
            Show(queue[0]);
            queue.RemoveAt(0);
        }
        List<TextData> delete = new List<TextData>();
        foreach(TextData data in texts) {
            data.Time = Mathf.Max(0.0f, data.Time - Time.deltaTime);
            data.Text.transform.position = new Vector2(starting_position.x, starting_position.y + (Max_Height * (1.0f - (data.Time / Message_Time))));
            if(data.Time == 0.0f) {
                delete.Add(data);
            }
        }
        foreach(TextData del in delete) {
            GameObject.Destroy(del.Text);
            texts.Remove(del);
        }
    }

    public void Show(string message)
    {
        if(current_cooldown != 0.0f) {
            queue.Add(message);
            return;
        }
        current_cooldown = Cooldown;
        GameObject game_object = GameObject.Instantiate(
            Text_Prototype,
            Text_Prototype.transform.position,
            Quaternion.identity,
            Container.transform
        );
        game_object.name = string.Format("Text#{0}", current_id);
        game_object.SetActive(true);
        game_object.GetComponentInChildren<Text>().text = message;
        current_id++;
        texts.Add(new TextData() { Text = game_object, Time = Message_Time });
    }

    public bool Active
    {
        get {
            return Container.activeSelf;
        }
        set {
            Container.SetActive(value);
        }
    }

    private class TextData
    {
        public GameObject Text { get; set; }
        public float Time { get; set; }
    }
}

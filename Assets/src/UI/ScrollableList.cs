using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScrollableList : MonoBehaviour {
    private static long current_id = 0;

    public GameObject Row_Prototype;
    public Button Upper_Button;
    public Button Lower_Button;
    public int Row_Height;

    private Dictionary<string, GameObject> rows;
    private int scroll_delta;

	private void Start () {
        Row_Prototype.SetActive(false);
    }
	
	private void Update () {
		
	}

    /// <summary>
    /// TODO: Is there a better solution for this?
    /// </summary>
    private void Initialize()
    {
        if(rows != null) {
            return;
        }
        rows = new Dictionary<string, GameObject>();
        scroll_delta = 0;

        Button.ButtonClickedEvent click_1 = new Button.ButtonClickedEvent();
        click_1.AddListener(new UnityAction(Scroll_Up));
        Upper_Button.onClick = click_1;

        Button.ButtonClickedEvent click_2 = new Button.ButtonClickedEvent();
        click_2.AddListener(new UnityAction(Scroll_Down));
        Lower_Button.onClick = click_2;
    }

    public void Clear()
    {
        Initialize();
        foreach(KeyValuePair<string, GameObject> pair in rows) {
            GameObject.Destroy(pair.Value);
        }
        rows.Clear();
    }

    public void Add_Row(string id, List<TextData> texts, List<ImageData> images, List<ButtonData> buttons, int index = -1)
    {
        Initialize();
        if (rows.ContainsKey(id)) {
            CustomLogger.Instance.Error(string.Format("Duplicate id: {0}", id));
            return;
        }
        index = index != -1 ? index : rows.Count;
        GameObject new_row = GameObject.Instantiate(
            Row_Prototype,
            new Vector3(
                Row_Prototype.transform.position.x,
                Row_Prototype.transform.position.y - ((index + scroll_delta) * Row_Height),
                Row_Prototype.transform.position.z
            ),
            Quaternion.identity,
            gameObject.transform
        );
        new_row.SetActive(true);
        new_row.name = string.Format("Row_{0}_#{1}", id, current_id);
        current_id++;
        Update_Row(new_row, texts, images, buttons);

        if(index != rows.Count) {
            Dictionary<string, GameObject> keep_position = new Dictionary<string, GameObject>();
            Dictionary<string, GameObject> shift_down = new Dictionary<string, GameObject>();
            int i = 0;
            foreach(KeyValuePair<string, GameObject> pair in rows) {
                if (i > index) {
                    shift_down.Add(pair.Key, pair.Value);
                } else if (i == index) {
                    keep_position.Add(id, new_row);
                    shift_down.Add(pair.Key, pair.Value);
                } else {
                    keep_position.Add(pair.Key, pair.Value);
                }
                i++;
            }
            rows.Clear();
            foreach(KeyValuePair<string, GameObject> pair in keep_position) {
                rows.Add(pair.Key, pair.Value);
            }
            foreach (KeyValuePair<string, GameObject> pair in shift_down) {
                pair.Value.transform.position = new Vector3(
                    pair.Value.transform.position.x,
                    pair.Value.transform.position.y - Row_Height,
                    pair.Value.transform.position.z
                );
                rows.Add(pair.Key, pair.Value);
            }
        } else {
            rows.Add(id, new_row);
        }

        Update_Row_Visibility();
    }

    public void Update_Row(string id, List<TextData> texts, List<ImageData> images, List<ButtonData> buttons)
    {
        Initialize();
        if (!rows.ContainsKey(id)) {
            CustomLogger.Instance.Error(string.Format("Row does not exist: {0}", id));
            return;
        }
        Update_Row(rows[id], texts, images, buttons);
    }

    public int Index_Of(string id)
    {
        Initialize();
        int index = 0;
        foreach(KeyValuePair<string, GameObject> pair in rows) {
            if(pair.Key == id) {
                return index;
            }
            index++;
        }
        //TODO: Log warning?
        return index;
    }

    public void Delete_Row(string id)
    {
        Initialize();
        if (!rows.ContainsKey(id)) {
            CustomLogger.Instance.Error(string.Format("Row does not exist: {0}", id));
            return;
        }

        int index = Index_Of(id);
        GameObject.Destroy(rows[id]);
        rows.Remove(id);
        if(index == rows.Count) {
            return;
        }

        //TODO: duplicate code
        Dictionary<string, GameObject> keep_position = new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> shift_up = new Dictionary<string, GameObject>();
        int i = 0;
        foreach (KeyValuePair<string, GameObject> pair in rows) {
            if (i >= index) {
                shift_up.Add(pair.Key, pair.Value);
            } else {
                keep_position.Add(pair.Key, pair.Value);
            }
            i++;
        }
        rows.Clear();
        foreach (KeyValuePair<string, GameObject> pair in keep_position) {
            rows.Add(pair.Key, pair.Value);
        }
        foreach (KeyValuePair<string, GameObject> pair in shift_up) {
            pair.Value.transform.position = new Vector3(
                pair.Value.transform.position.x,
                pair.Value.transform.position.y + Row_Height,
                pair.Value.transform.position.z
            );
            rows.Add(pair.Key, pair.Value);
        }
        Update_Row_Visibility();
    }

    private void Update_Row(GameObject row, List<TextData> texts, List<ImageData> images, List<ButtonData> buttons)
    {
        texts = texts == null ? new List<TextData>() : texts;
        images = images == null ? new List<ImageData>() : images;
        buttons = buttons == null ? new List<ButtonData>() : buttons;

        foreach (TextData data in texts) {
            Text text = GameObject.Find(string.Format("{0}/{1}/{2}", gameObject.name, row.name, data.Name)).GetComponent<Text>();
            if (text == null) {
                CustomLogger.Instance.Error(string.Format("Text not found: {0}", data.Name));
            } else {
                text.text = data.Text;
                text.gameObject.SetActive(data.Active);
            }
        }
        foreach(ImageData data in images) {
            Image image = GameObject.Find(string.Format("{0}/{1}/{2}", gameObject.name, row.name, data.Name)).GetComponent<Image>();
            if (image == null) {
                CustomLogger.Instance.Error(string.Format("Image not found: {0}", data.Name));
            } else {
                image.sprite = SpriteManager.Instance.Get_Sprite(data.Sprite_Name, data.Sprite_Type);
                image.gameObject.SetActive(data.Active);
            }
        }
        foreach (ButtonData data in buttons) {
            Button button = GameObject.Find(string.Format("{0}/{1}/{2}", gameObject.name, row.name, data.Name)).GetComponent<Button>();
            if (button == null) {
                CustomLogger.Instance.Error(string.Format("Button not found: {0}", data.Name));
            } else {
                Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
                click.AddListener(new UnityAction(data.Action));
                button.onClick = click;
                button.gameObject.SetActive(data.Active);
                button.interactable = data.Interactable;
                if(data.Text != null) {
                    Text button_text = GameObject.Find(string.Format("{0}/{1}/{2}", gameObject.name, row.name, data.Name)).GetComponentInChildren<Text>();
                    if(button_text == null) {
                        CustomLogger.Instance.Error(string.Format("Text for button not found: {0}", data.Name));
                    } else {
                        button_text.text = data.Text;
                    }
                }
            }
        }
    }

    private void Update_Row_Positions()
    {
        int index = 0;
        foreach (KeyValuePair<string, GameObject> pair in rows) {
            pair.Value.transform.position = new Vector3(
                Row_Prototype.transform.position.x,
                Row_Prototype.transform.position.y - ((index + scroll_delta) * Row_Height),
                Row_Prototype.transform.position.z
            );
            index++;
        }
        Update_Row_Visibility();
    }

    private void Update_Row_Visibility()
    {
        float size_y = gameObject.GetComponent<RectTransform>().rect.height;
        foreach(KeyValuePair<string, GameObject> pair in rows) {
            float y = pair.Value.GetComponent<RectTransform>().anchoredPosition.y;
            pair.Value.SetActive(y <= 0.0f && y >= (-size_y + Row_Height));
        }
    }
    
    public void Scroll_Up()
    {
        Initialize();
        scroll_delta++;
        Update_Row_Positions();
    }

    public void Scroll_Down()
    {
        Initialize();
        scroll_delta--;
        Update_Row_Positions();
    }

    public void Reset_Scroll()
    {
        Initialize();
        scroll_delta = 0;
        Update_Row_Positions();
    }

    public class TextData
    {
        public string Name { get; private set; }
        public string Text { get; private set; }
        public bool Active { get; private set; }

        public TextData(string name, string text, bool active)
        {
            Name = name;
            Text = text;
            Active = active;
        }
    }

    public class ImageData
    {
        public string Name { get; private set; }
        public string Sprite_Name { get; private set; }
        public SpriteManager.SpriteType Sprite_Type { get; private set; }
        public bool Active { get; private set; }

        public ImageData(string name, string sprite, SpriteManager.SpriteType type, bool active)
        {
            Name = name;
            Sprite_Name = sprite;
            Sprite_Type = type;
            Active = active;
        }
    }

    public class ButtonData
    {
        public delegate void ButtonClickDelegate();

        public string Name { get; private set; }
        public string Text { get; private set; }
        public ButtonClickDelegate Action { get; private set; }
        public bool Interactable { get; private set; }
        public bool Active { get; private set; }

        public ButtonData(string name, string text, ButtonClickDelegate action, bool interactable, bool active)
        {
            Name = name;
            Text = text;
            Action = action;
            Interactable = interactable;
            Active = active;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CraftingMenuManager : MonoBehaviour {
    public enum TabType { Refining, Tools }
    public static CraftingMenuManager Instance { get; private set; }

    public GameObject Main_Panel;
    public Button Tab_Button_Prototype;
    public GameObject Tab_Panel_Prototype;

    public GameObject Side_Panel;
    public Text Recipe_Name_Text;
    public Text Time_Text;
    public GameObject Requirement_Row_Prototype;
    public GameObject Input_Row_Prototype;
    public GameObject Output_Row_Prototype;

    public Slider Progress_Slider;
    public Text Progress_Text;
    public Button Craft_Button;

    private Dictionary<TabType, Button> tab_buttons;
    private Dictionary<TabType, GameObject> tab_panels;
    private CraftingRecipe current_recipe;
    private List<GameObject> requirement_rows;
    private List<GameObject> input_rows;
    private List<GameObject> output_rows;

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
        Main_Panel.SetActive(false);
        Side_Panel.SetActive(false);
        Tab_Button_Prototype.gameObject.SetActive(false);
        Tab_Panel_Prototype.SetActive(false);
        Requirement_Row_Prototype.SetActive(false);
        Input_Row_Prototype.SetActive(false);
        Output_Row_Prototype.SetActive(false);
        tab_buttons = new Dictionary<TabType, Button>();
        tab_panels = new Dictionary<TabType, GameObject>();
        current_recipe = null;

        Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
        click.AddListener(new UnityAction(Craft));
        Craft_Button.onClick = click;

        requirement_rows = new List<GameObject>();
        input_rows = new List<GameObject>();
        output_rows = new List<GameObject>();
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (Side_Panel.activeSelf) {
            if (Player.Current.Current_Crafting_Progress.HasValue) {
                Progress_Slider.value = Player.Current.Current_Crafting_Progress.Value;
                Progress_Text.text = string.Format("{0}%", Helper.Float_To_String(Player.Current.Current_Crafting_Progress.Value * 100.0f, 0));
            } else {
                Progress_Slider.value = 0.0f;
                Progress_Text.text = string.Empty;
            }
        }
    }

    public bool Active
    {
        get {
            return Main_Panel.activeSelf;
        }
        set {
            if (Main_Panel.activeSelf == value || value && Player.Current == null) {
                return;
            }
            Main_Panel.SetActive(value);
            Side_Panel.SetActive(false);
            Progress_Slider.value = 0.0f;
            Progress_Text.text = string.Empty;
            current_recipe = null;
            if (Active) {
                MainMenuManager.Instance.Active = false;
                BuildMenuManager.Instance.Active = false;
                BuildMenuManager.Instance.Preview_Active = false;
                InventoryGUIManager.Instance.Active = false;
                Update_GUI();
                Select_Tab(TabType.Refining);
            }
        }
    }

    public void Toggle()
    {
        Active = !Active;
    }

    public void Update_GUI()
    {
        foreach(KeyValuePair<TabType, Button> pair in tab_buttons) {
            GameObject.Destroy(pair.Value.gameObject);
        }
        tab_buttons.Clear();
        foreach(KeyValuePair<TabType, GameObject> pair in tab_panels) {
            GameObject.Destroy(pair.Value);
        }
        tab_panels.Clear();

        int index = 0;
        foreach(TabType tab in Enum.GetValues(typeof(TabType))) {
            //Button
            Button button = GameObject.Instantiate(
                Tab_Button_Prototype,
                new Vector3(
                    Tab_Button_Prototype.transform.position.x + (Tab_Button_Prototype.GetComponent<RectTransform>().rect.width * index),
                    Tab_Button_Prototype.transform.position.y,
                    Tab_Button_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Main_Panel.transform
            );
            button.name = string.Format("{0}_button", tab.ToString());
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<Text>().text = tab.ToString();

            Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
            click.AddListener(new UnityAction(delegate() { Select_Tab(tab); }));
            button.onClick = click;
            tab_buttons.Add(tab, button);

            //Panel
            GameObject panel = GameObject.Instantiate(
                Tab_Panel_Prototype,
                new Vector3(
                    Tab_Panel_Prototype.transform.position.x,
                    Tab_Panel_Prototype.transform.position.y,
                    Tab_Panel_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Main_Panel.transform
            );
            panel.name = string.Format("{0}_panel", tab.ToString());
            panel.SetActive(true);
            tab_panels.Add(tab, panel);

            //Recipes
            string s;
            foreach(CraftingRecipe recipe in CraftingRecipePrototypes.Instance.Get_All_In(tab)) {
                ScrollableList list = panel.GetComponentInChildren<ScrollableList>();
                list.Add_Row(recipe.Id.ToString(), new List<ScrollableList.TextData>() {
                    new ScrollableList.TextData("NameText", Player.Current.Can_Craft(recipe, out s) ? recipe.Name : string.Format("<i>{0}</i>", recipe.Name), true)
                },
                new List<ScrollableList.ImageData>() {
                    new ScrollableList.ImageData("IconImage", recipe.Icon_Sprite, recipe.Icon_Sprite_Type, true)
                },
                new List<ScrollableList.ButtonData>() {
                    new ScrollableList.ButtonData("SelectButton", null, delegate() { Select_Recipe(recipe); }, true, true)
                });
            }
            panel.SetActive(false);

            index++;
        }
    }

    public void Select_Tab(TabType tab)
    {
        foreach(KeyValuePair<TabType, GameObject> pair in tab_panels) {
            pair.Value.SetActive(pair.Key == tab);
        }
    }

    public void Select_Recipe(CraftingRecipe recipe)
    {
        current_recipe = recipe;
        Update_Side_Panel();
    }

    public void Craft()
    {
        if(current_recipe == null) {
            return;
        }
        string message;
        if(!Player.Current.Craft(current_recipe, out message)) {
            MessageManager.Instance.Show_Message(message);
        } else {
            Update_Side_Panel();
        }
    }

    public void Update_Side_Panel()
    {
        Side_Panel.SetActive(current_recipe != null);
        if (!Side_Panel.activeSelf) {
            return;
        }
        foreach(GameObject row in requirement_rows) {
            GameObject.Destroy(row);
        }
        requirement_rows.Clear();
        foreach (GameObject row in input_rows) {
            GameObject.Destroy(row);
        }
        input_rows.Clear();
        foreach (GameObject row in output_rows) {
            GameObject.Destroy(row);
        }
        output_rows.Clear();

        Recipe_Name_Text.text = current_recipe.Name;

        Time_Text.text = string.Format("Time: {0}s", Helper.Float_To_String(Player.Current.Estimated_Crafting_Time(current_recipe), 1));
        TooltipManager.Instance.Register_Tooltip(Time_Text.gameObject, string.Format("Base: {0}s", current_recipe.Time), Time_Text.gameObject);

        foreach(KeyValuePair<Skill.SkillId, int> pair in current_recipe.Required_Skills) {
            GameObject row = GameObject.Instantiate(
                Requirement_Row_Prototype,
                new Vector3(
                    Requirement_Row_Prototype.transform.position.x,
                    Requirement_Row_Prototype.transform.position.y - (20.0f * requirement_rows.Count),
                    Requirement_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Side_Panel.transform
            );
            row.SetActive(true);
            row.GetComponentInChildren<Image>().gameObject.SetActive(false);
            row.GetComponentInChildren<Text>().text = string.Format("Level {0} {1}", pair.Value, pair.Key);
            if(!Player.Current.Has_Skill(pair.Key, pair.Value)) {
                row.GetComponentInChildren<Text>().color = Color.red;
            }
            requirement_rows.Add(row);
        }

        foreach (KeyValuePair<Tool.ToolType, int> pair in current_recipe.Required_Tools) {
            GameObject row = GameObject.Instantiate(
                Requirement_Row_Prototype,
                new Vector3(
                    Requirement_Row_Prototype.transform.position.x,
                    Requirement_Row_Prototype.transform.position.y - (20.0f * requirement_rows.Count),
                    Requirement_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Side_Panel.transform
            );
            row.SetActive(true);
            row.GetComponentInChildren<Image>().gameObject.SetActive(false);
            row.GetComponentInChildren<Text>().text = string.Format("Level {0} {1}", pair.Value, pair.Key);
            if (!Player.Current.Has_Tool(pair.Key, pair.Value)) {
                row.GetComponentInChildren<Text>().color = Color.red;
            }
            requirement_rows.Add(row);
        }

        foreach (KeyValuePair<string, int> pair in current_recipe.Inputs) {
            GameObject row = GameObject.Instantiate(
                Input_Row_Prototype,
                new Vector3(
                    Input_Row_Prototype.transform.position.x,
                    Input_Row_Prototype.transform.position.y - (20.0f * input_rows.Count),
                    Input_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Side_Panel.transform
            );
            row.SetActive(true);
            Item prototype = ItemPrototypes.Instance.Get_Item_Prototype(pair.Key);
            row.GetComponentInChildren<Image>().sprite = SpriteManager.Instance.Get_Sprite(prototype.UI_Sprite, prototype.UI_Sprite_Type);
            row.GetComponentInChildren<Text>().text = string.Format("{0}x {1}", pair.Value, prototype.Name);
            if (!Player.Current.Has_Items(pair.Key, pair.Value)) {
                row.GetComponentInChildren<Text>().color = Color.red;
            }
            input_rows.Add(row);
        }

        foreach (KeyValuePair<string, int> pair in current_recipe.Outputs) {
            GameObject row = GameObject.Instantiate(
                Output_Row_Prototype,
                new Vector3(
                    Output_Row_Prototype.transform.position.x,
                    Output_Row_Prototype.transform.position.y - (20.0f * output_rows.Count),
                    Output_Row_Prototype.transform.position.z
                ),
                Quaternion.identity,
                Side_Panel.transform
            );
            row.SetActive(true);
            Item prototype = ItemPrototypes.Instance.Get_Item_Prototype(pair.Key);
            row.GetComponentInChildren<Image>().sprite = SpriteManager.Instance.Get_Sprite(prototype.UI_Sprite, prototype.UI_Sprite_Type);
            row.GetComponentInChildren<Text>().text = string.Format("{0}x {1}", pair.Value, prototype.Name);
            output_rows.Add(row);
        }

        string message = null;
        if (Player.Current.Can_Craft(current_recipe, out message)) {
            Craft_Button.interactable = true;
            TooltipManager.Instance.Unregister_Tooltip(Craft_Button.gameObject);
        } else {
            Craft_Button.interactable = false;
            TooltipManager.Instance.Register_Tooltip(Craft_Button.gameObject, message, Craft_Button.gameObject);
        }
    }
}

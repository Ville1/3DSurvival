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
    public Slider Progress_Slider;
    public Text Progress_Text;
    public Button Craft_Button;

    private Dictionary<TabType, Button> tab_buttons;
    private Dictionary<TabType, GameObject> tab_panels;
    private CraftingRecipe current_recipe;

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
        tab_buttons = new Dictionary<TabType, Button>();
        tab_panels = new Dictionary<TabType, GameObject>();
        current_recipe = null;

        Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
        click.AddListener(new UnityAction(Craft));
        Craft_Button.onClick = click;
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
            foreach(CraftingRecipe recipe in CraftingRecipePrototypes.Instance.Get_All_In(tab)) {
                ScrollableList list = panel.GetComponentInChildren<ScrollableList>();
                list.Add_Row(recipe.Id.ToString(), new List<ScrollableList.TextData>() {
                    new ScrollableList.TextData("NameText", recipe.Name, true)
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
        Side_Panel.SetActive(true);
        Recipe_Name_Text.text = recipe.Name;
        string message = null;
        if (Player.Current.Can_Craft(recipe, out message)) {
            Craft_Button.interactable = true;
            TooltipManager.Instance.Unregister_Tooltip(Craft_Button.gameObject);
        } else {
            Craft_Button.interactable = false;
            TooltipManager.Instance.Register_Tooltip(Craft_Button.gameObject, message, Craft_Button.gameObject);
        }
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
        if(current_recipe != null) {
            Select_Recipe(current_recipe);
        }
    }
}

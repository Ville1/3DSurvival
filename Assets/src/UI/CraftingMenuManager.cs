using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CraftingMenuManager : MonoBehaviour {
    public enum TabType { Misc, Test1 }
    public static CraftingMenuManager Instance { get; private set; }

    public GameObject Main_Panel;
    public Button Tab_Button_Prototype;
    public GameObject Tab_Panel_Prototype;

    public GameObject Side_Panel;
    public Text Recipe_Name_Text;
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
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {

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
            current_recipe = null;
            MouseManager.Instance.Show_Cursor = value;
            if (Active) {
                MainMenuManager.Instance.Visible = false;
                BuildMenuManager.Instance.Active = false;
                BuildMenuManager.Instance.Preview_Active = false;
                InventoryGUIManager.Instance.Active = false;
                Update_GUI();
                Select_Tab(TabType.Misc);
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
        Side_Panel.SetActive(true);
        Recipe_Name_Text.text = recipe.Name;
    }
}

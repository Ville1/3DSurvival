using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildMenuManager : MonoBehaviour
{
    public enum TabType { Misc }

    public static BuildMenuManager Instance { get; private set; }

    public GameObject Main_Panel;
    public Button Tab_Button_Prototype;
    public GameObject Tab_Panel_Prototype;
    public GameObject Item_Prototype;

    public GameObject Side_Panel;
    public Text Item_Name_Text;
    public Button Build_Button;

    private bool initialized;
    private bool preview_active;
    private Dictionary<TabType, GameObject> panels;
    private Block selected_block;
    private Block preview_block;

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
        Item_Prototype.SetActive(false);
        preview_active = false;
        initialized = false;
        selected_block = null;
        preview_block = null;

        Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
        click.AddListener(new UnityAction(Build));
        Build_Button.onClick = click;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if(Player.Current != null && preview_active) {
            if(preview_block == null) {
                preview_block = new Block(new Coordinates(0, 0, 0), selected_block, Map.Instance.Block_Container, null, true);
            }
            Block hover_block = MouseManager.Instance.Object_Under_Cursor != null && MouseManager.Instance.Object_Under_Cursor is Block ? MouseManager.Instance.Object_Under_Cursor as Block : null;
            if(hover_block != null) {
                Block location_block = Map.Instance.Get_Block_At(hover_block.Coordinates.Shift(new Coordinates(MouseManager.Instance.Hit_Normal)));
                preview_block.Position = location_block.Position;
                if (Input.GetMouseButtonDown(0)) {
                    //TODO: Move to mouse manager?
                    string message = null;
                    if (Player.Current.Can_Build(selected_block, location_block, out message)) {
                        Player.Current.Build_Block(selected_block, location_block, out message);
                        Preview_Active = false;
                    } else {
                        MessageManager.Instance.Show_Message(message);
                    }
                }
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
            if (Active) {
                MainMenuManager.Instance.Visible = false;
                InventoryGUIManager.Instance.Active = false;
                CraftingMenuManager.Instance.Active = false;
                Update_GUI();
                Select_Tab(TabType.Misc);
                Preview_Active = false;
            }
        }
    }

    public bool Preview_Active
    {
        get {
            return preview_active;
        }
        set {
            preview_active = value;
            if (!preview_active) {
                if(preview_block != null) {
                    preview_block.Delete();
                    preview_block = null;
                    selected_block = null;
                }
            }
        }
    }

    public void Toggle()
    {
        Active = !Active;
    }

    private void Update_GUI()
    {
        if (initialized) {
            return;
        }
        initialized = true;
        panels = new Dictionary<TabType, GameObject>();
        int index = 0;
        foreach(TabType tab in Enum.GetValues(typeof(TabType))) {
            //Panel
            GameObject panel = GameObject.Instantiate(
                Tab_Panel_Prototype,
                Tab_Panel_Prototype.transform.position,
                Quaternion.identity,
                Main_Panel.transform
            );
            panel.name = string.Format("{0}_panel", tab.ToString());
            panel.SetActive(false);
            panels.Add(tab, panel);

            //Panel button
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
            button.gameObject.SetActive(true);
            button.name = string.Format("{0}_button", tab.ToString());
            Button.ButtonClickedEvent click = new Button.ButtonClickedEvent();
            click.AddListener(new UnityAction(delegate() { Select_Tab(tab); }));
            button.onClick = click;
            button.GetComponentInChildren<Text>().text = tab.ToString();

            //Items
            List<Block> blocks = BlockPrototypes.Instance.Get_Blocks_In(tab);
            int row = 0;
            int column = 0;
            int row_max_columns = 6;
            foreach (Block block in blocks) {
                GameObject item = GameObject.Instantiate(
                    Item_Prototype,
                    new Vector3(
                        Item_Prototype.transform.position.x + (Item_Prototype.GetComponent<RectTransform>().rect.width * column),
                        Item_Prototype.transform.position.y - (Item_Prototype.GetComponent<RectTransform>().rect.height * row),
                        Item_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    panel.transform
                );
                item.gameObject.SetActive(true);
                item.name = string.Format("{0}_item", block.Internal_Name);

                item.GetComponentInChildren<Text>().text = block.Name;
                item.GetComponentInChildren<Image>().sprite = SpriteManager.Instance.Get_Sprite(block.UI_Sprite, block.UI_Sprite_Type);

                Button.ButtonClickedEvent item_click = new Button.ButtonClickedEvent();
                item_click.AddListener(new UnityAction(delegate () { Select_Item(block); }));
                item.GetComponentInChildren<Button>().onClick = item_click;

                column++;
                if(column > row_max_columns) {
                    column = 0;
                    row++;
                }
            }
            index++;
        }
    }

    private void Select_Tab(TabType tab)
    {
        foreach(KeyValuePair<TabType, GameObject> pair in panels) {
            pair.Value.SetActive(pair.Key == tab);
        }
    }

    private void Select_Item(Block item)
    {
        Side_Panel.SetActive(true);
        Item_Name_Text.text = item.Name;
        string message = null;
        selected_block = item;
        if(!Player.Current.Can_Build(item, out message)) {
            Build_Button.interactable = false;
            TooltipManager.Instance.Register_Tooltip(Build_Button.gameObject, message, Build_Button.gameObject);
        } else {
            Build_Button.interactable = true;
            TooltipManager.Instance.Unregister_Tooltip(Build_Button.gameObject);
        }
    }

    private void Build()
    {
        if(selected_block == null) {
            return;
        }
        preview_active = true;
        Active = false;
    }
}

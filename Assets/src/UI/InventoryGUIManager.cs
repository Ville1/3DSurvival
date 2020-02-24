using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryGUIManager : MonoBehaviour {
    public static InventoryGUIManager Instance { get; private set; }

    public GameObject List_Panel;

    public GameObject Info_Panel;
    public Text Volyme_Text;
    public Text Weight_Text;

    public GameObject Item_Panel;

    private Item current_item;
    private ScrollableList list;

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
        current_item = null;
        list = List_Panel.GetComponentInChildren<ScrollableList>();
        List_Panel.SetActive(false);
        Info_Panel.SetActive(false);
        Item_Panel.SetActive(false);
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
            return List_Panel.activeSelf;
        }
        set {
            if(List_Panel.activeSelf == value || value && Player.Current == null) {
                return;
            }
            List_Panel.SetActive(value);
            Info_Panel.SetActive(value);
            Item_Panel.SetActive(false);
            current_item = null;
            MouseManager.Instance.Show_Cursor = value;
            if (Active) {
                Update_GUI();
            }
        }
    }

    public void Toggle()
    {
        Active = !Active;
    }

    private void Update_GUI()
    {
        list.Clear();
        Volyme_Text.text = string.Format("Volyme: {0}", !Inventory.Limited_Volyme ? "Unlimited" : string.Format("{0} / {1}", Helper.Float_To_String(Inventory.Current_Volyme, 1), Inventory.Max_Volyme));
        Weight_Text.text = string.Format("Weight: {0}", !Inventory.Limited_Weight ? "Unlimited" : string.Format("{0} / {1}", Helper.Float_To_String(Inventory.Current_Weight, 1), Inventory.Max_Weight));

        foreach(KeyValuePair<string, int> stacked_items in Inventory.Count_Dictionary_Internal_Names) {
            Item item = null;
            if(stacked_items.Value == 1) {
                item = Inventory.Get_Items(stacked_items.Key)[0];
            } else {
                item = ItemPrototypes.Instance.Get_Item(stacked_items.Key);
            }
            list.Add_Row(item.Internal_Name,
                new List<ScrollableList.TextData>() {
                    new ScrollableList.TextData("CountText", string.Format("{0}x", stacked_items.Value), stacked_items.Value != 1),
                    new ScrollableList.TextData("NameText", item.Name, true),
                    new ScrollableList.TextData("DurabilityText", string.Format("{0}%", item.Unbreaking ? "inf" : Helper.Float_To_String(item.Relative_Durability * 100.0f, 0)), stacked_items.Value == 1)
                },
                new List<ScrollableList.ImageData>() {
                    new ScrollableList.ImageData("IconImage", item.UI_Sprite, item.UI_Sprite_Type, true)
                },
                new List<ScrollableList.ButtonData>() {
                    new ScrollableList.ButtonData("ExpandButton", ">", delegate() { Expand(item, stacked_items.Value); }, true, stacked_items.Value != 1),
                    new ScrollableList.ButtonData("InvisibleButton", null, delegate() { Select_Item(item,  stacked_items.Value != 1); }, true, true)
                });
        }
    }

    private void Expand(Item prototype_item, int count)
    {
        list.Update_Row(prototype_item.Internal_Name,
            new List<ScrollableList.TextData>() {
                new ScrollableList.TextData("CountText", string.Format("{0}x", count), true),
                new ScrollableList.TextData("NameText", prototype_item.Name, true),
                new ScrollableList.TextData("DurabilityText", null, false)
            },
            new List<ScrollableList.ImageData>() {
                new ScrollableList.ImageData("IconImage", prototype_item.UI_Sprite, prototype_item.UI_Sprite_Type, true)
            },
            new List<ScrollableList.ButtonData>() {
                new ScrollableList.ButtonData("ExpandButton", "V", delegate() { Contract(prototype_item, count); }, true, true),
                new ScrollableList.ButtonData("InvisibleButton", null, delegate() { Select_Item(prototype_item, true); }, true, true)
            });
        int index = 1;
        foreach(Item item in Inventory.Get_Items(prototype_item.Internal_Name)) {
            list.Add_Row(item.Id.ToString(),
                new List<ScrollableList.TextData>() {
                    new ScrollableList.TextData("CountText", null, false),
                    new ScrollableList.TextData("NameText", item.Name, true),
                    new ScrollableList.TextData("DurabilityText", string.Format("{0}%", item.Unbreaking ? "inf" : Helper.Float_To_String(item.Relative_Durability * 100.0f, 0)), true)
                },
                new List<ScrollableList.ImageData>() {
                    new ScrollableList.ImageData("IconImage", item.UI_Sprite, item.UI_Sprite_Type, true)
                },
                new List<ScrollableList.ButtonData>() {
                    new ScrollableList.ButtonData("ExpandButton", ">", delegate() { }, false, false),
                    new ScrollableList.ButtonData("InvisibleButton", null, delegate() { Select_Item(item, false); }, true, true)
                }, list.Index_Of(prototype_item.Internal_Name) + index);
            index++;
        }
    }

    private void Contract(Item item_prototype, int count)
    {
        list.Update_Row(item_prototype.Internal_Name,
            new List<ScrollableList.TextData>() {
                new ScrollableList.TextData("CountText", string.Format("{0}x", count), true),
                new ScrollableList.TextData("NameText", item_prototype.Name, true),
                new ScrollableList.TextData("DurabilityText", null, false)
            },
            new List<ScrollableList.ImageData>() {
                new ScrollableList.ImageData("IconImage", item_prototype.UI_Sprite, item_prototype.UI_Sprite_Type, true)
            },
            new List<ScrollableList.ButtonData>() {
                new ScrollableList.ButtonData("ExpandButton", ">", delegate() { Expand(item_prototype, count); }, true, true),
                new ScrollableList.ButtonData("InvisibleButton", null, delegate() { Select_Item(item_prototype, true); }, true, true)
            });

        foreach (Item item in Inventory.Get_Items(item_prototype.Internal_Name)) {
            list.Delete_Row(item.Id.ToString());
        }
    }

    private void Select_Item(Item item, bool is_stack)
    {
        CustomLogger.Instance.Debug(is_stack.ToString());
    }

    private Inventory Inventory
    {
        get {
            return Player.Current.Inventory;
        }
    }
}

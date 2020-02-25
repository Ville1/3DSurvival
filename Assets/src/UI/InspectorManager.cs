using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class InspectorManager : MonoBehaviour
{
    public static InspectorManager Instance { get; private set; }

    public GameObject Panel;
    public Image Image;
    public Text Name_Text;

    public GameObject Block_Data_Container;
    public Text Block_Coordinates_Text;
    public Text Block_HP_Text;
    public Text Block_Actions_Text;

    public GameObject Mob_Data_Container;
    public Text Mob_HP_Text;

    public GameObject Item_Pile_Data_Container;
    public Text Item_Pile_Row_1_Text;
    public Text Item_Pile_Row_2_Text;
    public Text Item_Pile_Row_3_Text;
    public Text Item_Pile_Row_4_Text;
    public Text Item_Pile_Row_5_Text;

    private MapObject target;

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
        Active = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!Active) {
            return;
        }
        Block_Data_Container.SetActive(false);
        Mob_Data_Container.SetActive(false);
        Item_Pile_Data_Container.SetActive(false);
        if (Target is Block) {
            Block block = Target as Block;
            Block_Data_Container.SetActive(true);
            Name_Text.text = block.Name;
            Image.sprite = SpriteManager.Instance.Get_Sprite(block.UI_Sprite, block.UI_Sprite_Type);
            Block_Coordinates_Text.text = block.Coordinates.Parse_Text(true, true);
            Block_HP_Text.text = string.Format("HP: {0} / {1}", Helper.Float_To_String(block.HP, 0), block.MAX_HP);

            Dictionary<string, string> actions = new Dictionary<string, string>();
            List<string> possible_actions = new List<string>();
            string s = null;
            if (!block.Indestructible) {
                string dismantle_button = KeyboardManager.Instance.Get_Key_Bind("Dismantle block");
                actions.Add(dismantle_button, block.Dismantle_Verb.Base);
                if (Player.Current.Can_Dismantle(block, out s, true)) {
                    possible_actions.Add(dismantle_button);
                }
                if (block.Can_Be_Repaired) {
                    string repair_button = KeyboardManager.Instance.Get_Key_Bind("Repair block");
                    actions.Add(repair_button, "Repair");
                    if (Player.Current.Can_Repair(block, out s, true)) {
                        possible_actions.Add(repair_button);
                    }
                }
                if (block.Harvestable) {
                    string harvest_button = KeyboardManager.Instance.Get_Key_Bind("Harvest block");
                    actions.Add(harvest_button, block.Harvest_Verb.Base);
                    if (Player.Current.Can_Harvest(block, out s, true)) {
                        possible_actions.Add(harvest_button);
                    }
                }
            }
            if(actions.Count != 0) {
                StringBuilder builder = new StringBuilder();
                foreach(KeyValuePair<string, string> pair in actions) {
                    if (!possible_actions.Contains(pair.Key)) {
                        builder.Append("<i>");
                    }
                    builder.Append(pair.Key).Append(" = ").Append(pair.Value);
                    if (!possible_actions.Contains(pair.Key)) {
                        builder.Append("</i>");
                    }
                    builder.Append(", ");
                }
                Block_Actions_Text.text = builder.Remove(builder.Length - 2, 2).ToString();
            } else {
                Block_Actions_Text.text = string.Empty;
            }
        } else if(Target is Mob) {
            Mob_Data_Container.SetActive(true);
            Mob mob = Target as Mob;
            Name_Text.text = mob.Name;
            Image.sprite = SpriteManager.Instance.Get_Sprite(SpriteManager.UI_PLACEHOLDER_NAME, SpriteManager.SpriteType.UI);
            Block_HP_Text.text = string.Format("HP: {0} / {1}", Helper.Float_To_String(mob.HP, 0), mob.MAX_HP);
        } else if (Target is ItemPile) {
            Item_Pile_Data_Container.SetActive(true);
            ItemPile pile = Target as ItemPile;
            Name_Text.text = Helper.Plural("Item", pile.Inventory.Count);
            Item top_most = pile.Top_Most_Item;
            if(top_most != null) {
                Image.sprite = SpriteManager.Instance.Get_Sprite(top_most.UI_Sprite, top_most.UI_Sprite_Type);
            } else {
                Image.sprite = SpriteManager.Instance.Get_Sprite(SpriteManager.UI_PLACEHOLDER_NAME, SpriteManager.SpriteType.UI);
            }
            Item_Pile_Row_1_Text.gameObject.SetActive(false);
            Item_Pile_Row_2_Text.gameObject.SetActive(false);
            Item_Pile_Row_3_Text.gameObject.SetActive(false);
            Item_Pile_Row_4_Text.gameObject.SetActive(false);
            Item_Pile_Row_5_Text.gameObject.SetActive(false);
            Dictionary<string, int> counts = pile.Inventory.Count_Dictionary_Names.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            int row = 1;
            int total = 0;
            //TODO: Messy code
            foreach(KeyValuePair<string, int> data in counts) {
                if(row == 1) {
                    Item_Pile_Row_1_Text.gameObject.SetActive(true);
                    Item_Pile_Row_1_Text.text = string.Format("{0}x {1}", data.Value, data.Key);
                    total += data.Value;
                } else if(row == 2) {
                    Item_Pile_Row_2_Text.gameObject.SetActive(true);
                    Item_Pile_Row_2_Text.text = string.Format("{0}x {1}", data.Value, data.Key);
                    total += data.Value;
                } else if (row == 3) {
                    Item_Pile_Row_3_Text.gameObject.SetActive(true);
                    Item_Pile_Row_3_Text.text = string.Format("{0}x {1}", data.Value, data.Key);
                    total += data.Value;
                } else if (row == 4) {
                    Item_Pile_Row_4_Text.gameObject.SetActive(true);
                    Item_Pile_Row_4_Text.text = string.Format("{0}x {1}", data.Value, data.Key);
                    total += data.Value;
                } else if (row == 5) {
                    Item_Pile_Row_5_Text.gameObject.SetActive(true);
                    Item_Pile_Row_5_Text.text = string.Format("{0}x {1}", data.Value, data.Key);
                } else {
                    Item_Pile_Row_5_Text.text = string.Format("+{0} other item{1}", pile.Inventory.Count - total, Helper.Plural(pile.Inventory.Count - total));
                    break;
                }
                row++;
            }
        }
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            Panel.SetActive(value);
        }
    }

    public MapObject Target
    {
        get {
            return target;
        }
        set {
            target = value;
            Active = target != null;
        }
    }

    public Block Block
    {
        get {
            return target == null || !Active || !(target is Block) ? null : target as Block;
        }
    }
}

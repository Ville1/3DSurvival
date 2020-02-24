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

    public GameObject Mob_Data_Container;
    public Text Mob_HP_Text;

    public GameObject Item_Pile_Data_Container;

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
        } else if(Target is Mob) {
            Mob_Data_Container.SetActive(true);
            Mob mob = Target as Mob;
            Name_Text.text = mob.Name;
            Image.sprite = SpriteManager.Instance.Get_Sprite(SpriteManager.UI_PLACEHOLDER_NAME, SpriteManager.SpriteType.UI);
            Block_HP_Text.text = string.Format("HP: {0} / {1}", Helper.Float_To_String(mob.HP, 0), mob.MAX_HP);
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

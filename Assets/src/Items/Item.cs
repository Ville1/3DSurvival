public class Item {
    private static long current_id;

    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Internal_Name { get; private set; }
    public float Durability { get; private set; }
    public int Max_Durability { get; private set; }
    public string UI_Sprite { get; private set; }
    public SpriteManager.SpriteType UI_Sprite_Type { get; private set; }
    public bool Unbreaking { get { return Max_Durability <= 0.0f; } }
    public float Weight { get; private set; }
    public float Volyme { get; private set; }

    public float Relative_Durability { get { return Unbreaking ? 1.0f : Durability / Max_Durability; } }

    public Item(Item prototype)
    {
        Id = current_id;
        current_id++;
        Change_To(prototype, prototype.Max_Durability);
    }

    public Item(ItemSaveData data)
    {
        Id = current_id;
        current_id++;
        Change_To(ItemPrototypes.Instance.Get_Item(data.Internal_Name), data.Durability);
    }

    public Item(string name, string internal_name, int durability, float weight, float volyme, string ui_sprite, SpriteManager.SpriteType ui_sprite_type)
    {
        Name = name;
        Internal_Name = internal_name;
        Durability = -1;
        Max_Durability = durability;
        Weight = weight;
        Volyme = volyme;
        UI_Sprite = ui_sprite;
        UI_Sprite_Type = ui_sprite_type;
    }

    public ItemSaveData Save_Data
    {
        get {
            return new ItemSaveData {
                Internal_Name = Internal_Name,
                Durability = Durability
            };
        }
    }

    private void Change_To(Item prototype, float durability)
    {
        Name = prototype.Name;
        Internal_Name = prototype.Internal_Name;
        Durability = durability;
        Max_Durability = prototype.Max_Durability;
        Weight = prototype.Weight;
        Volyme = prototype.Volyme;
        UI_Sprite = prototype.UI_Sprite;
        UI_Sprite_Type = prototype.UI_Sprite_Type;
    }

    public override string ToString()
    {
        return string.Format("{0} #{1}", Internal_Name, Id);
    }
}

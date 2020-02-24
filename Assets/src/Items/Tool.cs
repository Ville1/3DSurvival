public class Tool : Item {
    public enum ToolType { Pickaxe, Hammer }

    public ToolType Type { get; private set; }
    public int Level { get; private set; }
    public float Base_Efficiency { get; private set; }

    public Tool(Tool prototype) : base(prototype)
    {
        Type = prototype.Type;
        Level = prototype.Level;
        Base_Efficiency = prototype.Base_Efficiency;
    }

    public Tool(string name, string internal_name, int durability, float weight, float volyme, string ui_sprite, SpriteManager.SpriteType ui_sprite_type, ToolType type, int level, float efficiency) :
        base(name, internal_name, durability, weight, volyme, ui_sprite, ui_sprite_type)
    {
        Type = type;
        Level = level;
        Base_Efficiency = efficiency;
    }

    public float Efficiency
    {
        get {
            return Relative_Durability >= 0.5f ? Base_Efficiency : (0.5f * Base_Efficiency) + (0.5f * Base_Efficiency * Relative_Durability);
        }
    }
}

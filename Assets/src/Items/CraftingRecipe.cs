using System.Collections.Generic;

public class CraftingRecipe  {
    private static long current_id = 0;

    public long Id { get; private set; }
    public string Name { get; private set; }
    public string Internal_Name { get; private set; }
    public CraftingMenuManager.TabType Tab { get; private set; }
    public Dictionary<string, int> Inputs { get; private set; }
    public Dictionary<string, int> Outputs { get; private set; }
    public Dictionary<Tool.ToolType, int> Required_Tools { get; private set; }
    public Dictionary<Skill.SkillId, int> Required_Skills { get; private set; }
    public string Icon_Sprite { get; private set; }
    public SpriteManager.SpriteType Icon_Sprite_Type { get; private set; }

    public CraftingRecipe(string name, string internal_name, CraftingMenuManager.TabType tab, Dictionary<string, int> inputs, Dictionary<string, int> outputs, Dictionary<Tool.ToolType, int> required_tools,
        Dictionary<Skill.SkillId, int> required_skills, string icon_sprite, SpriteManager.SpriteType icon_sprite_type)
    {
        Id = current_id;
        current_id++;
        Name = name;
        Internal_Name = internal_name;
        Tab = tab;
        Inputs = inputs != null ? Helper.Clone_Dictionary(inputs) : new Dictionary<string, int>();
        Outputs = outputs != null ? Helper.Clone_Dictionary(outputs) : new Dictionary<string, int>();
        Required_Tools = required_tools != null ? Helper.Clone_Dictionary(required_tools) : new Dictionary<Tool.ToolType, int>();
        Required_Skills = required_skills != null ? Helper.Clone_Dictionary(required_skills) : new Dictionary<Skill.SkillId, int>();
        Icon_Sprite = icon_sprite;
        Icon_Sprite_Type = icon_sprite_type;
    }

    public override string ToString()
    {
        return string.Format("{0} (#{1})", Internal_Name, Id);
    }
}

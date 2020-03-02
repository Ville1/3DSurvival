using System.Collections.Generic;
using System.Linq;

public class CraftingRecipePrototypes {
    private static CraftingRecipePrototypes instance;

    private List<CraftingRecipe> prototypes;

    private CraftingRecipePrototypes()
    {
        prototypes = new List<CraftingRecipe>();

        prototypes.Add(new CraftingRecipe("Craft rope", "craft_rope", 2.5f, CraftingMenuManager.TabType.Refining, new Dictionary<string, int>() { { "plant_fiber", 2 } }, new Dictionary<string, int>() { { "rope", 1 } }, null, null, "placeholder", SpriteManager.SpriteType.UI));
        prototypes.Add(new CraftingRecipe("Sharpen flint", "sharpen_flint", 2.0f, CraftingMenuManager.TabType.Refining, new Dictionary<string, int>() { { "stone", 1 }, { "flint", 1 } }, new Dictionary<string, int>() { { "stone", 1 }, { "sharp_flint", 1 } }, null, null, "placeholder", SpriteManager.SpriteType.UI));
        prototypes.Add(new CraftingRecipe("Cut crude plank", "cut_crude_plank", 2.0f, CraftingMenuManager.TabType.Refining, new Dictionary<string, int>() { { "wood", 1 } }, new Dictionary<string, int>() { { "crude_plank", 1 } }, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Axe, 1 } }, null, "placeholder", SpriteManager.SpriteType.UI));

        prototypes.Add(new CraftingRecipe("Craft primitive axe", "craft_primitive_axe", 6.0f, CraftingMenuManager.TabType.Tools, new Dictionary<string, int>() { { "sharp_flint", 1 }, { "stick", 1 }, { "rope", 1 } }, new Dictionary<string, int>() { { "flint_axe", 1 } }, null, null, "placeholder", SpriteManager.SpriteType.UI));
        prototypes.Add(new CraftingRecipe("Craft primitive hammer", "craft_primitive_hammer", 5.0f, CraftingMenuManager.TabType.Tools, new Dictionary<string, int>() { { "stone", 1 }, { "stick", 1 }, { "rope", 1 } }, new Dictionary<string, int>() { { "stone_hammer", 1 } }, null, null, "placeholder", SpriteManager.SpriteType.UI));
        prototypes.Add(new CraftingRecipe("Craft primitive shovel", "craft_primitive_shovel", 5.0f, CraftingMenuManager.TabType.Tools, new Dictionary<string, int>() { { "crude_plank", 1 }, { "stick", 1 }, { "rope", 1 } }, new Dictionary<string, int>() { { "wooden_shovel", 1 } }, null, null, "placeholder", SpriteManager.SpriteType.UI));
    }

    public static CraftingRecipePrototypes Instance
    {
        get {
            if (instance == null) {
                instance = new CraftingRecipePrototypes();
            }
            return instance;
        }
    }

    public CraftingRecipe Get(string internal_name)
    {
        CraftingRecipe recipe = prototypes.FirstOrDefault(x => x.Internal_Name == internal_name);
        if (recipe == null) {
            CustomLogger.Instance.Error(string.Format("CraftingRecipe not found: {0}", internal_name));
            return null;
        }
        return recipe;
    }

    public List<CraftingRecipe> Get_All_In(CraftingMenuManager.TabType tab)
    {
        return prototypes.Where(x => x.Tab == tab).ToList();
    }
}

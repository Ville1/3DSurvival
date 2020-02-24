using System.Collections.Generic;
using System.Linq;

public class CraftingRecipePrototypes {
    private static CraftingRecipePrototypes instance;

    private List<CraftingRecipe> prototypes;

    private CraftingRecipePrototypes()
    {
        prototypes = new List<CraftingRecipe>();

        prototypes.Add(new CraftingRecipe("Test", "test", CraftingMenuManager.TabType.Misc, new Dictionary<string, int>() { { "stone", 2 } }, new Dictionary<string, int>() { { "wood", 1 } }, null, null, "placeholder", SpriteManager.SpriteType.UI));
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

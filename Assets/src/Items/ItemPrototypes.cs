using System.Collections.Generic;
using System.Linq;

public class ItemPrototypes
{
    private static ItemPrototypes instance;

    private List<Item> prototypes;

    private ItemPrototypes()
    {
        prototypes = new List<Item>();

        prototypes.Add(new Item("Stone", "stone", 100, 1.0f, 1.0f, "stone", SpriteManager.SpriteType.Item));
        prototypes.Add(new Item("Wood", "wood", 100, 1.0f, 1.0f, "wood", SpriteManager.SpriteType.Item));
        prototypes.Add(new Item("Plant fiber", "plant_fiber", 100, 1.0f, 1.0f, "placeholder", SpriteManager.SpriteType.Item));
        prototypes.Add(new Item("Dirt", "dirt", 100, 1.0f, 1.0f, "dirt", SpriteManager.SpriteType.Block));
        prototypes.Add(new Item("Stick", "stick", 100, 1.0f, 1.0f, "placeholder", SpriteManager.SpriteType.Item));
        prototypes.Add(new Item("Flint", "flint", 100, 1.0f, 1.0f, "placeholder", SpriteManager.SpriteType.Item));
        prototypes.Add(new Item("Crude plank", "crude_plank", 100, 1.0f, 1.0f, "placeholder", SpriteManager.SpriteType.Item));


        prototypes.Add(new Item("Rope", "rope", 100, 1.0f, 1.0f, "placeholder", SpriteManager.SpriteType.Item));
        prototypes.Add(new Item("Sharp flint", "sharp_flint", 100, 1.0f, 1.0f, "placeholder", SpriteManager.SpriteType.Item));

        prototypes.Add(new Tool("Flint axe", "flint_axe", 100, 0.0f, 0.0f, "placeholder", SpriteManager.SpriteType.Item, Tool.ToolType.Axe, 1, 1.0f));
        prototypes.Add(new Tool("Stone hammer", "stone_hammer", 100, 0.0f, 0.0f, "placeholder", SpriteManager.SpriteType.Item, Tool.ToolType.Hammer, 1, 1.0f));
        prototypes.Add(new Tool("Wooden shovel", "wooden_shovel", 100, 0.0f, 0.0f, "placeholder", SpriteManager.SpriteType.Item, Tool.ToolType.Shovel, 1, 1.0f));

        prototypes.Add(new Tool("Dev hammer", "dev_hammer", -1, 0.0f, 0.0f, "placeholder", SpriteManager.SpriteType.Item, Tool.ToolType.Hammer, 999, 50.0f));
        prototypes.Add(new Tool("Dev pickaxe", "dev_pickaxe", -1, 0.0f, 0.0f, "placeholder", SpriteManager.SpriteType.Item, Tool.ToolType.Pickaxe, 999, 50.0f));
        prototypes.Add(new Tool("Dev shovel", "dev_shovel", -1, 0.0f, 0.0f, "placeholder", SpriteManager.SpriteType.Item, Tool.ToolType.Shovel, 999, 50.0f));
    }

    public static ItemPrototypes Instance
    {
        get {
            if (instance == null) {
                instance = new ItemPrototypes();
            }
            return instance;
        }
    }

    /// <summary>
    /// Return a clone of prototype
    /// </summary>
    /// <param name="internal_name"></param>
    /// <returns></returns>
    public Item Get_Item(string internal_name)
    {
        Tool tool = (Tool)prototypes.FirstOrDefault(x => x is Tool && x.Internal_Name == internal_name);
        if (tool != null) {
            return new Tool(tool);
        }
        Item item = prototypes.FirstOrDefault(x => x.Internal_Name == internal_name);
        if (item == null) {
            CustomLogger.Instance.Error(string.Format("Item not found: {0}", internal_name));
            return null;
        }
        return new Item(item);
    }

    /// <summary>
    /// Return a clone of prototype
    /// </summary>
    /// <param name="internal_name"></param>
    /// <returns></returns>
    public Tool Get_Tool(string internal_name)
    {
        Tool tool = (Tool)prototypes.FirstOrDefault(x => x is Tool && x.Internal_Name == internal_name);
        if (tool == null) {
            CustomLogger.Instance.Error(string.Format("Tool not found: {0}", internal_name));
            return null;
        }
        return new Tool(tool);
    }

    public bool Is_Tool(string internal_name)
    {
        return prototypes.Exists(x => x is Tool && x.Internal_Name == internal_name);
    }
}

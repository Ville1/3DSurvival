using System.Collections.Generic;
using System.Linq;

public class BlockPrototypes {
    public static readonly string AIR_INTERNAL_NAME = "air";

    private static BlockPrototypes instance;

    private List<Block> prototypes;

    private BlockPrototypes()
    {
        prototypes = new List<Block>();

        Verb dismantle_verb = new Verb("Dismantle", "Dismantling");
        Verb mine_verb = new Verb("Mine", "Mining");
        Verb dig_verb = new Verb("Dig", "Digging");
        Verb remove_verb = new Verb("Remove", "Removing");
        Verb pickup_verb = new Verb("Pick up", "Picking up");
        Verb chop_verb = new Verb("Chop", "Chopping");

        Verb harvest_verb = new Verb("Harvest", "Harvesting");

        prototypes.Add(new Block("Air", AIR_INTERNAL_NAME, "air", null, true, true, true, -1, "placeholder", SpriteManager.SpriteType.UI, 1.0f, 1.0f, null, null, null, null, dismantle_verb, null, null, null, -1.0f, null, null, null, null, null, null, null, new ConnectionData(), false));

        prototypes.Add(new Block("Rock", "rock", "rock", null, false, false, false, 100, "rock", SpriteManager.SpriteType.Block, 1.0f, 1.0f, new Dictionary<string, int>() { { "stone", 1 } }, null,
            new Dictionary<Skill.SkillId, int>() { { Skill.SkillId.Mining, 1 } }, null, mine_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Pickaxe, 1 } }, null, null, -1.0f, null, null, null, null,
            null, null, null, new ConnectionData(true), false));

        prototypes.Add(new Block("Dirt", "dirt", "dirt", null, false, false, false, 50, "dirt", SpriteManager.SpriteType.Block, 1.0f, 100.0f, new Dictionary<string, int>() { { "dirt", 1 } }, null,
            null, null, dig_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Shovel, 1 } }, null, null, -1.0f, null, null, null, null, null, null, null, new ConnectionData(true), false));

        prototypes.Add(new Block("Bed rock", "bed_rock", "rock", null, false, false, false, -1, "placeholder", SpriteManager.SpriteType.UI, 1.0f, 1.0f, null, null,
            null, null, null, null, null, null, -1.0f, null, null, null, null, null, null, null, new ConnectionData(true), true));

        prototypes.Add(new Block("Grass", "grass", "grass", null, false, false, false, 50, "grass", SpriteManager.SpriteType.Block, 1.0f, 100.0f, new Dictionary<string, int>() { { "dirt", 1 } }, null,
            null, null, dig_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Shovel, 1 } }, null, null, -1.0f, null, null, null, null, null, null, null, new ConnectionData(true), false));
        prototypes.Add(new Block("Grass slope", "grass_slope", "grass", null, false, false, false, 25, "grass", SpriteManager.SpriteType.Block, 1.0f, 100.0f, new Dictionary<string, int>() { { "dirt", 1 } }, null,
            null, null, dig_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Shovel, 1 } }, null, null, -1.0f, null, null, null, null, null, null, null, new ConnectionData(true), false));

        prototypes.Add(new Block("Tall grass", "tall_grass", null, "tall_grass", true, true, false, 10, "placeholder", SpriteManager.SpriteType.UI, 1.0f, 100.0f, new Dictionary<string, int>() { { "plant_fiber", 3 } }, null, null, null, remove_verb, null, null, null,
            999.0f, "medium_grass", new Dictionary<string, int>() { { "plant_fiber", 1 } }, null, null, harvest_verb, delegate (Block block) { block.Persistent_Data.Add("original_lenght", "tall"); }, null, new ConnectionData(ConnectionData.Direction.Bottom), false));

        Block.UpdateDelegate medium_grass_grow = delegate (float delta_time, Block block) {
            if(block.Persistent_Data.ContainsKey("original_lenght") && (string)block.Persistent_Data["original_lenght"] == "medium") {
                return;
            }
            if (!block.Data.ContainsKey("growth_target")) {
                block.Data.Add("growth_target", (float)RNG.Instance.Next(7200, 18000));
                //block.Data.Add("growth_target", (float)RNG.Instance.Next(30, 60));
            }
            if (!block.Data.ContainsKey("growth")) {
                block.Data.Add("growth", delta_time);
            } else {
                block.Data["growth"] = (float)block.Data["growth"] + delta_time;
            }
            if ((float)block.Data["growth"] > (float)block.Data["growth_target"]) {
                block.Change_To(Instance.Get("tall_grass"));
            }
        };

        prototypes.Add(new Block("Medium grass", "medium_grass", null, "medium_grass", true, true, false, 5, "placeholder", SpriteManager.SpriteType.UI, 1.0f, 100.0f, new Dictionary<string, int>() { { "plant_fiber", 2 } }, null, null, null, remove_verb, null, null, null,
            999.0f, "short_grass", new Dictionary<string, int>() { { "plant_fiber", 1 } }, null, null, harvest_verb, delegate(Block block) { block.Persistent_Data.Add("original_lenght", "medium"); }, medium_grass_grow, new ConnectionData(ConnectionData.Direction.Bottom), false));

        Block.UpdateDelegate short_grass_grow = delegate (float delta_time, Block block) {
            if (block.Persistent_Data.ContainsKey("original_lenght") && (string)block.Persistent_Data["original_lenght"] == "short") {
                return;
            }
            if (!block.Data.ContainsKey("growth_target")) {
                block.Data.Add("growth_target", (float)RNG.Instance.Next(900, 1800));
                //block.Data.Add("growth_target", (float)RNG.Instance.Next(30, 60));
            }
            if (!block.Data.ContainsKey("growth")) {
                block.Data.Add("growth", delta_time);
            } else {
                block.Data["growth"] = (float)block.Data["growth"] + delta_time;
            }
            if((float)block.Data["growth"] > (float)block.Data["growth_target"]) {
                block.Change_To(Instance.Get("medium_grass"));
            }
        };

        prototypes.Add(new Block("Short grass", "short_grass", null, "short_grass", true, true, false, 1, "placeholder", SpriteManager.SpriteType.UI, 1.0f, 100.0f, new Dictionary<string, int>() { { "plant_fiber", 1 } }, null, null, null, remove_verb, null, null, null,
            -1.0f, null, null, null, null, harvest_verb, delegate (Block block) { block.Persistent_Data.Add("original_lenght", "short"); }, short_grass_grow, new ConnectionData(ConnectionData.Direction.Bottom), false));

        prototypes.Add(new Block("Stones", "stones", null, "stones", true, true, false, 10, "stone", SpriteManager.SpriteType.Item, -1.0f, 100.0f, new Dictionary<string, int>() { { "stone", 1 } }, null, null, null, dismantle_verb, null, null, null,
            999.0f, null, new Dictionary<string, int>() { { "stone", 1 } }, null, null, pickup_verb, null, null, new ConnectionData(ConnectionData.Direction.Bottom), false));
        prototypes.Add(new Block("Sticks", "sticks", null, "sticks", true, true, false, 10, "placeholder", SpriteManager.SpriteType.UI, -1.0f, 100.0f, new Dictionary<string, int>() { { "stick", 1 } }, null, null, null, dismantle_verb, null, null, null,
            999.0f, null, new Dictionary<string, int>() { { "stick", 1 } }, null, null, pickup_verb, null, null, new ConnectionData(ConnectionData.Direction.Bottom), false));
        prototypes.Add(new Block("Flint", "flint", null, "flint", true, true, false, 10, "placeholder", SpriteManager.SpriteType.UI, -1.0f, 100.0f, new Dictionary<string, int>() { { "flint", 1 } }, null, null, null, dismantle_verb, null, null, null,
            999.0f, null, new Dictionary<string, int>() { { "flint", 1 } }, null, null, pickup_verb, null, null, new ConnectionData(ConnectionData.Direction.Bottom), false));

        prototypes.Add(new Block("Stick wall", "stick_wall", "planks", null, false, false, false, 50, "placeholder", SpriteManager.SpriteType.UI, 10.0f, 100.0f, new Dictionary<string, int>() { { "stick", 1 }, { "rope", 1 } },
            new Dictionary<string, int>() { { "stick", 3 }, { "rope", 2 } }, null, null, dismantle_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Hammer, 1 }, { Tool.ToolType.Axe, 1 } },
            new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Hammer, 1 } }, BuildMenuManager.TabType.Misc, -1.0f, null, null, null, null, null, null, null, new ConnectionData(true), false));

        prototypes.Add(new Block("Tree trunk", "trunk", null, "trunk", false, false, false, 50, "placeholder", SpriteManager.SpriteType.UI, 10.0f, 100.0f, new Dictionary<string, int>() { { "wood", 1 } }, null, null, null, chop_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Axe, 1 } }, null, null,
            -1.0f, null, null, null, null, null, null, null, new ConnectionData(new List<ConnectionData.Direction>() { ConnectionData.Direction.Bottom, ConnectionData.Direction.Top }), false));
        prototypes.Add(new Block("Leaves", "leaves", "leaves", null, false, false, false, 5, "leaves", SpriteManager.SpriteType.Block, 1.0f, 100.0f, new Dictionary<string, int>() { { "stick", 1 } }, null,
            null, null, remove_verb, null, null, null, -1.0f, null, null, null, null, null, null, null, new ConnectionData(true), false));
    }

    public static BlockPrototypes Instance
    {
        get {
            if(instance == null) {
                instance = new BlockPrototypes();
            }
            return instance;
        }
    }

    public Block Get(string internal_name)
    {
        Block block = prototypes.FirstOrDefault(x => x.Internal_Name == internal_name);
        if(block == null) {
            CustomLogger.Instance.Error(string.Format("Block not found: {0}", internal_name));
            return null;
        }
        return block;
    }

    public Block Air
    {
        get {
            return Get(AIR_INTERNAL_NAME);
        }
    }

    public List<Block> Get_Blocks_In(BuildMenuManager.TabType tab)
    {
        return prototypes.Where(x => x.Build_Menu_Tab == tab).OrderBy(x => x.Name).ToList();
    }
}

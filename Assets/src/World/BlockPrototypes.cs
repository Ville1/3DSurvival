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

        Verb harvest_verb = new Verb("Harvest", "Harvesting");

        prototypes.Add(new Block("Air", AIR_INTERNAL_NAME, "air", null, true, true, true, -1, "placeholder", SpriteManager.SpriteType.UI, 1.0f, 1.0f, null, null, null, null, dismantle_verb, null, null, null, -1.0f, null, null, null, null, null, null, null));

        prototypes.Add(new Block("Rock", "rock", "rock", null, false, false, false, 100, "rock", SpriteManager.SpriteType.Block, 1.0f, 1.0f, new Dictionary<string, int>() { { "stone", 1 } }, null,
            new Dictionary<Skill.SkillId, int>() { { Skill.SkillId.Mining, 1 } }, null, mine_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Pickaxe, 1 } }, null, null, -1.0f, null, null, null, null, null, null, null));

        prototypes.Add(new Block("Dirt", "dirt", "dirt", null, false, false, false, 50, "dirt", SpriteManager.SpriteType.Block, 1.0f, 100.0f, new Dictionary<string, int>() { { "dirt", 1 } }, null,
            null, null, dig_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Shovel, 1 } }, null, null, -1.0f, null, null, null, null, null, null, null));

        prototypes.Add(new Block("Grass", "grass", "grass", null, false, false, false, 50, "grass", SpriteManager.SpriteType.Block, 1.0f, 100.0f, new Dictionary<string, int>() { { "dirt", 1 } }, null,
            null, null, dig_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Shovel, 1 } }, null, null, -1.0f, null, null, null, null, null, null, null));

        prototypes.Add(new Block("Tall grass", "tall_grass", null, "tall_grass", true, true, false, 10, "placeholder", SpriteManager.SpriteType.UI, -1.0f, 100.0f, null, null, null, null, dismantle_verb, null, null, null,
            999.0f, "medium_grass", new Dictionary<string, int>() { { "plant_fiber", 1 } }, null, null, harvest_verb, delegate (Block block) { block.Persistent_Data.Add("original_lenght", "tall"); }, null));

        Block.UpdateDelegate medium_grass_grow = delegate (float delta_time, Block block) {
            if(block.Persistent_Data.ContainsKey("original_lenght") && (string)block.Persistent_Data["original_lenght"] == "medium") {
                return;
            }
            if (!block.Data.ContainsKey("growth_target")) {
                //block.Data.Add("growth_target", (float)RNG.Instance.Next(7200, 18000));
                block.Data.Add("growth_target", (float)RNG.Instance.Next(30, 60));
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

        prototypes.Add(new Block("Medium grass", "medium_grass", null, "medium_grass", true, true, false, 5, "placeholder", SpriteManager.SpriteType.UI, -1.0f, 100.0f, null, null, null, null, dismantle_verb, null, null, null,
            999.0f, "short_grass", new Dictionary<string, int>() { { "plant_fiber", 1 } }, null, null, harvest_verb, delegate(Block block) { block.Persistent_Data.Add("original_lenght", "medium"); }, medium_grass_grow));

        Block.UpdateDelegate short_grass_grow = delegate (float delta_time, Block block) {
            if (block.Persistent_Data.ContainsKey("original_lenght") && (string)block.Persistent_Data["original_lenght"] == "short") {
                return;
            }
            if (!block.Data.ContainsKey("growth_target")) {
                //block.Data.Add("growth_target", (float)RNG.Instance.Next(900, 1800));
                block.Data.Add("growth_target", (float)RNG.Instance.Next(30, 60));
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

        prototypes.Add(new Block("Short grass", "short_grass", null, "short_grass", true, true, false, 1, "placeholder", SpriteManager.SpriteType.UI, -1.0f, 100.0f, null, null, null, null, dismantle_verb, null, null, null,
            -1.0f, null, null, null, null, harvest_verb, delegate (Block block) { block.Persistent_Data.Add("original_lenght", "short"); }, short_grass_grow));
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

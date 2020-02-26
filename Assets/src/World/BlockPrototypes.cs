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

        prototypes.Add(new Block("Air", AIR_INTERNAL_NAME, "air", null, true, true, true, -1, "placeholder", SpriteManager.SpriteType.UI, 1.0f, 1.0f, null, null, null, null, dismantle_verb, null, null, null, -1.0f, null, null, null, null, null));

        prototypes.Add(new Block("Rock", "rock", "rock", null, false, false, false, 100, "rock", SpriteManager.SpriteType.Block, 1.0f, 1.0f, new Dictionary<string, int>() { { "stone", 1 } }, null,
            new Dictionary<Skill.SkillId, int>() { { Skill.SkillId.Mining, 1 } }, null, mine_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Pickaxe, 1 } }, null, null, -1.0f, null, null, null, null, null));

        prototypes.Add(new Block("Dirt", "dirt", "dirt", null, false, false, false, 50, "dirt", SpriteManager.SpriteType.Block, 1.0f, 100.0f, new Dictionary<string, int>() { { "dirt", 1 } }, null,
            null, null, dig_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Shovel, 1 } }, null, null, -1.0f, null, null, null, null, null));

        prototypes.Add(new Block("Grass", "grass", "grass", null, false, false, false, 50, "grass", SpriteManager.SpriteType.Block, 1.0f, 100.0f, new Dictionary<string, int>() { { "dirt", 1 } }, null,
            null, null, dig_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Shovel, 1 } }, null, null, -1.0f, null, null, null, null, null));

        prototypes.Add(new Block("Tall grass", "tall_grass", null, "grass", true, true, false, 1, "placeholder", SpriteManager.SpriteType.UI, -1.0f, 100.0f, null, null, null, null, dismantle_verb, null, null, null,
            999.0f, null, new Dictionary<string, int>() { { "plant_fiber", 1 } }, null, null, harvest_verb));
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

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

        Verb harvest_verb = new Verb("Harvest", "Harvesting");

        prototypes.Add(new Block("Rock", "rock", "rock", false, false, false, 100, "rock", SpriteManager.SpriteType.Block, 1.0f, 1.0f, new Dictionary<string, int>() { { "stone", 1 } },
            new Dictionary<string, int>() { { "stone", 1 } }, new Dictionary<Skill.SkillId, int>() { { Skill.SkillId.Mining, 1 } }, new Dictionary<Skill.SkillId, int>() { { Skill.SkillId.Masonry, 1 } },
            mine_verb, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Pickaxe, 1 } }, new Dictionary<Tool.ToolType, int>() { { Tool.ToolType.Hammer, 1 } }, BuildMenuManager.TabType.Misc, 50.0f, "planks",
            new Dictionary<string, int>() { { "wood", 1 } }, null, null, harvest_verb));
        prototypes.Add(new Block("Planks", "planks", "planks", false, false, false, 100, "planks", SpriteManager.SpriteType.Block, 1.0f, 1.0f, null, null, null, null, dismantle_verb, null, null, null, -1.0f, null, null, null, null, null));
        prototypes.Add(new Block("Air", AIR_INTERNAL_NAME, "air", true, true, true, -1, "placeholder", SpriteManager.SpriteType.UI, 1.0f, 1.0f, null, null, null, null, dismantle_verb, null, null, null, -1.0f, null, null, null, null, null));
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

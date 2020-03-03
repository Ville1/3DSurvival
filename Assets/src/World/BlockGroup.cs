using System.Collections.Generic;
using System.Linq;

public class BlockGroup {
    public enum GroupType { Tree }

    private static long current_id = 0;

    public long Id { get; private set; }
    public string Name { get; private set; }
    public GroupType Type { get; private set; }
    public List<Block> Blocks { get; set; }

    public BlockGroup(string name, GroupType type)
    {
        Id = current_id;
        current_id++;
        Name = name;
        Type = type;
        Blocks = new List<Block>();
    }
    
    public BlockGroupSaveData Save_Data
    {
        get {
            return new BlockGroupSaveData() {
                Id = Id,
                Name = Name,
                Type = (int)Type,
                Blocks = Blocks.Select(x => x.Id).ToList()
            };
        }
    }

    public void Add(Block block)
    {
        Blocks.Add(block);
        if (!block.Groups.Contains(this)) {
            block.Groups.Add(this);
        }
    }

    public static BlockGroup Load(BlockGroupSaveData data, Chunk chunk)
    {
        if (current_id <= data.Id) {
            current_id = data.Id + 1;
        }
        BlockGroup group = new BlockGroup(data.Name, (GroupType)data.Type);
        group.Id = data.Id;
        foreach(long id in data.Blocks) {
            Block block = chunk.Blocks.FirstOrDefault(x => x.Id == id);
            if(block == null) {
                CustomLogger.Instance.Error(string.Format("Block not found: #{0}", id));
                continue;
            }
            group.Blocks.Add(block);
            block.Groups.Add(group);
        }
        return group;
    }

    public static void Reset_Current_Id()
    {
        current_id = 0;
    }
}

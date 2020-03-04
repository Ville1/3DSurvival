using System.Collections.Generic;
using System.Linq;

public class StructuralIntegrityManager {
    public static readonly float COOLDOWN = 0.25f;
    public static readonly int MAX_UPDATES = 1000;

    private Map map;

    private float current_cooldown;
    private Block current_root;
    private List<Block> current_lump;
    private List<Block> root_queue;
    private List<Block> check_next;
    private List<Block> demolish;
    private int current_update;
    private List<long> last_stable;

    public StructuralIntegrityManager(Map map)
    {
        this.map = map;
        current_cooldown = COOLDOWN;
        current_lump = new List<Block>();
        root_queue = new List<Block>();
        check_next = new List<Block>();
        demolish = new List<Block>();
        last_stable = new List<long>();
    }

    public void Update(float delta_time)
    {
        if (!map.Structural_Integrity_Enabled) {
            return;
        }
        current_cooldown -= delta_time;
        if(current_cooldown > 0.0f) {
            return;
        }
        current_cooldown += COOLDOWN;

        if(demolish.Count != 0) {
            Block block = demolish[0];
            demolish.RemoveAt(0);
            block.Deal_Damage(float.MaxValue, false, true);
            return;
        }

        if(current_root == null) {
            if(root_queue.Count == 0) {
                return;
            }
            current_root = root_queue[0];
            root_queue.RemoveAt(0);
            current_lump.Clear();
            current_lump.Add(current_root);
            check_next.Clear();
            check_next.Add(current_root);
            demolish.Clear();
            current_update = 0;
        }
        Update_Integrity();
    }

    public void Check(Block block)
    {
        if (!map.Structural_Integrity_Enabled || root_queue.Contains(block)) {
            return;
        }
        root_queue.Add(block);
    }

    private void Update_Integrity()
    {
        if(check_next.Count == 0) {
            demolish = Helper.Clone_List(current_lump);
            current_root = null;
            return;
        }
        Block block = check_next[0];
        check_next.RemoveAt(0);
        foreach(Block connected_block in block.Get_Connected_Blocks()) {
            if (current_lump.Contains(connected_block)) {
                continue;
            }
            if(connected_block.Base_Pilar_Support || connected_block.Base_Support || last_stable.Exists(x => x == connected_block.Id)) {
                last_stable = current_lump.Select(x => x.Id).ToList();
                current_root = null;
                return;
            }
            current_lump.Add(connected_block);
            check_next.Add(connected_block);
        }
        current_update++;
        if(current_update == MAX_UPDATES) {
            CustomLogger.Instance.Warning("Max updates reached");
            last_stable = current_lump.Select(x => x.Id).ToList();
            current_root = null;
        }
    }
}

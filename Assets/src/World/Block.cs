using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Block : MapObject {
    public static readonly string GAME_OBJECT_NAME_PREFIX = "Block_";
    public static readonly string TRANSPARENT_MATERIAL_PREFIX = "transparent_";
    public static readonly string PREFAB_NAME = "Block";
    public static readonly string SLOPE_PREFAB_NAME = "Slope";
    public static readonly float UPDATE_INTERVAL = 30.0f;
    public static readonly int UPDATE_STRUCTURAL_INTERGRITY_RANGE = 20;
    public static readonly int UPDATE_STRUCTURAL_INTERGRITY_MAX_CALLS = 50;

    public static bool Log_Diagnostics = true;

    public delegate void UpdateDelegate(float delta_time, Block block);
    public delegate void CreateDelegate(Block block);

    private static long current_id = 0;
    private static readonly string[] CRACK_MATERIALS = new string[3] { "cracks_1", "cracks_2", "cracks_3" };

    public long Id { get; private set; }
    public string Internal_Name { get; private set; }
    public bool Passable { get; private set; }
    public bool Inactive_GameObject { get; private set; }
    public bool Indestructible { get; private set; }
    public bool Can_Be_Built_Over { get; private set; }
    public int MAX_HP { get; private set; }
    public float HP { get; private set; }
    public float HP_Dismantled { get; private set; }
    public float HP_Built { get; private set; }
    public string UI_Sprite { get; private set; }
    public SpriteManager.SpriteType UI_Sprite_Type { get; private set; }
    public float Dismantle_Speed { get; private set; }
    public bool Can_Be_Dismantled { get { return Dismantle_Speed > 0.0f; } }
    public float Build_Speed { get; private set; }
    public float Harvest_Speed { get; private set; }
    public bool Harvestable { get { return !Indestructible && Harvest_Speed > 0.0f; } }
    public bool Instant_Harvest { get { return !Indestructible && Harvest_Speed >= 100.0f; } }
    public string After_Harvest_Prototype { get; private set; }
    public float Harvest_Progress { get; private set; }
    public float Harvest_Progress_Relative { get { return Harvestable ? Harvest_Progress / MAX_HP : 0.0f; } }
    public Verb Harvest_Verb { get; private set; }
    public Dictionary<string, int> Harvest_Drops { get; private set; }
    public Dictionary<string, int> Dismantle_Drops { get; private set; }
    public Dictionary<string, int> Materials_Required_To_Build { get; private set; }
    public Dictionary<Skill.SkillId, int> Skills_Required_To_Dismantle { get; private set; }
    public Dictionary<Skill.SkillId, int> Skills_Required_To_Build { get; private set; }
    public Dictionary<Tool.ToolType, int> Tools_Required_To_Dismantle { get; private set; }
    public Dictionary<Tool.ToolType, int> Tools_Required_To_Build { get; private set; }
    public Dictionary<Skill.SkillId, int> Skills_Required_To_Harvest { get; private set; }
    public Dictionary<Tool.ToolType, int> Tools_Required_To_Harvest { get; private set; }
    public Verb Dismantle_Verb { get; private set; }
    public float Relative_HP { get { return Indestructible ? 1.0f : HP / MAX_HP; } }
    public bool Completed { get; private set; }
    public BuildMenuManager.TabType? Build_Menu_Tab { get; private set; }
    public bool Buildable { get { return Build_Menu_Tab.HasValue; } }
    public bool Preview { get; private set; }
    public bool Can_Be_Repaired { get { return Relative_HP != 1.0f || Harvest_Progress != 0.0f; } }
    public Chunk Chunk { get; set; }
    public Dictionary<string, object> Data { get; private set; }
    public Dictionary<string, object> Persistent_Data { get; private set; }
    public UpdateDelegate Update_Action { get; private set; }
    public CreateDelegate Create_Action { get; private set; }
    public bool Supports_Top { get { return Connections != null ? Connections.Top : false; } }
    public bool Is_Air { get { return Internal_Name == BlockPrototypes.AIR_INTERNAL_NAME; } }
    public ConnectionData Connections { get; private set; }
    public bool Base_Support { get; private set; }
    public bool Base_Pilar_Support { get; set; }

    private ConnectionData last_connections;
    private GameObject crack_cube;
    private float update_cooldown;
    private int update_structural_integrity_calls;
    private string watch_title;
    private Dictionary<string, Stopwatch> watches;

    public Block(Coordinates position, Block prototype, GameObject container, float? hp = null, bool is_preview = false) : base(prototype.Name, position.Vector, container, prototype.Prototype_Data, !prototype.Inactive_GameObject)
    {
        Id = current_id;
        current_id++;
        //TODO: Move to Change_To?
        Connections = new ConnectionData(prototype.Connections);
        Base_Pilar_Support = false;

        Change_To(prototype, false, hp, is_preview);

        GameObject.name = string.Format("{0}#{1}_{2}", GAME_OBJECT_NAME_PREFIX, Id, Coordinates.Parse_Text(true, false));
        foreach(Transform transform in GameObject.transform) {
            if(transform.name == "CrackCube") {
                crack_cube = transform.gameObject;
            }
        }
        Map.Instance.Add_Block(this);
        Update_Material();
        if (is_preview) {
            GameObject.GetComponentInChildren<BoxCollider>().enabled = false;
        }
        if(Create_Action != null) {
            Create_Action(this);
        }
    }

    //TODO: internal_name.Contains("_slope")
    public Block(string name, string internal_name, string material, string model_name, bool passable, bool can_be_built_over, bool inactive, int hp, string ui_sprite, SpriteManager.SpriteType ui_sprite_type, float dismantle_speed,
        float build_speed, Dictionary<string, int> dismantle_drops, Dictionary<string, int> building_materials, Dictionary<Skill.SkillId, int> dismantle_skills, Dictionary<Skill.SkillId, int> build_skills,
        Verb dismantle_verb, Dictionary<Tool.ToolType, int> tools_required_to_dismantle, Dictionary<Tool.ToolType, int> tools_required_to_build, BuildMenuManager.TabType? build_menu_tab, float harvest_speed,
        string after_harvest_prototype, Dictionary<string, int> harvest_drops, Dictionary<Skill.SkillId, int> skills_required_to_harvest, Dictionary<Tool.ToolType, int> tools_required_to_harvest, Verb harvest_verb,
        CreateDelegate create_action, UpdateDelegate update_action, ConnectionData connections, bool base_support) : 
        base(name, string.IsNullOrEmpty(model_name) ? (internal_name.Contains("_slope") ? SLOPE_PREFAB_NAME : PREFAB_NAME) : null, string.IsNullOrEmpty(model_name) ? material : null, MaterialManager.MaterialType.Block, model_name)
    {
        Id = -1;
        Name = name;
        Internal_Name = internal_name;
        Material = material;
        Passable = passable;
        Can_Be_Built_Over = can_be_built_over;
        Inactive_GameObject = inactive;
        MAX_HP = hp;
        Indestructible = hp <= 0.0f;
        HP = -1.0f;
        HP_Dismantled = -1.0f;
        HP_Built = -1.0f;
        UI_Sprite = ui_sprite;
        UI_Sprite_Type = ui_sprite_type;
        Dismantle_Speed = dismantle_speed;
        Build_Speed = build_speed;
        Dismantle_Drops = dismantle_drops != null ? Helper.Clone_Dictionary(dismantle_drops) : new Dictionary<string, int>();
        Materials_Required_To_Build = building_materials != null ? Helper.Clone_Dictionary(building_materials) : new Dictionary<string, int>();
        Skills_Required_To_Dismantle = dismantle_skills != null ? Helper.Clone_Dictionary(dismantle_skills) : new Dictionary<Skill.SkillId, int>();
        Skills_Required_To_Build = build_skills != null ? Helper.Clone_Dictionary(build_skills) : new Dictionary<Skill.SkillId, int>();
        Dismantle_Verb = new Verb(dismantle_verb);
        Tools_Required_To_Dismantle = tools_required_to_dismantle != null ? Helper.Clone_Dictionary(tools_required_to_dismantle) : new Dictionary<Tool.ToolType, int>();
        Tools_Required_To_Build = tools_required_to_build != null ? Helper.Clone_Dictionary(tools_required_to_build) : new Dictionary<Tool.ToolType, int>();
        Completed = true;
        Build_Menu_Tab = build_menu_tab;
        Harvest_Speed = harvest_speed;
        After_Harvest_Prototype = after_harvest_prototype;
        Harvest_Progress = -1.0f;
        Harvest_Drops = harvest_drops != null ? Helper.Clone_Dictionary(harvest_drops) : new Dictionary<string, int>();
        Skills_Required_To_Harvest = skills_required_to_harvest != null ? Helper.Clone_Dictionary(skills_required_to_harvest) : new Dictionary<Skill.SkillId, int>();
        Tools_Required_To_Harvest = tools_required_to_harvest != null ? Helper.Clone_Dictionary(tools_required_to_harvest) : new Dictionary<Tool.ToolType, int>();
        Harvest_Verb = new Verb(harvest_verb);
        Data = new Dictionary<string, object>();
        Persistent_Data = new Dictionary<string, object>();
        Update_Action = update_action;
        update_cooldown = -1.0f;
        Create_Action = create_action;
        Connections = new ConnectionData(connections);
        last_connections = new ConnectionData(connections);
        Base_Support = base_support;
        Base_Pilar_Support = false;
    }

    public Coordinates Coordinates
    {
        get {
            return new Coordinates(Position);
        }
    }

    public static long? Parse_Id_From_GameObject_Name(string name)
    {
        if(!name.Contains(GAME_OBJECT_NAME_PREFIX) || !name.Contains("#")) {
            return null;
        }
        long id = 0;
        string sub = name.Substring(name.IndexOf("#") + 1);
        if (!long.TryParse(sub.Substring(0, sub.IndexOf("_")), out id)) {
            CustomLogger.Instance.Error(string.Format("Error parsing string: {0}", name));
            return null;
        }
        return id;
    }

    public void Change_To(Block prototype, bool update_material = true, float? hp = null, bool is_preview = false)
    {
        bool changed_prefab = Prefab_Name != prototype.Prefab_Name;
        Name = prototype.Name;
        Internal_Name = prototype.Internal_Name;
        Material = prototype.Material;
        Prefab_Name = prototype.Prefab_Name;
        Model_Name = prototype.Model_Name;
        Passable = prototype.Passable;
        Can_Be_Built_Over = prototype.Can_Be_Built_Over;
        Inactive_GameObject = prototype.Inactive_GameObject;
        HP = hp.HasValue ? hp.Value : prototype.MAX_HP;
        MAX_HP = prototype.MAX_HP;
        HP_Dismantled = 0.0f;
        HP_Built = 0.0f;
        Indestructible = prototype.Indestructible;
        UI_Sprite = prototype.UI_Sprite;
        UI_Sprite_Type = prototype.UI_Sprite_Type;
        Dismantle_Speed = prototype.Dismantle_Speed;
        Build_Speed = prototype.Build_Speed;
        Dismantle_Drops = Helper.Clone_Dictionary(prototype.Dismantle_Drops);
        Materials_Required_To_Build = Helper.Clone_Dictionary(prototype.Materials_Required_To_Build);
        Skills_Required_To_Dismantle = Helper.Clone_Dictionary(prototype.Skills_Required_To_Dismantle);
        Skills_Required_To_Build = Helper.Clone_Dictionary(prototype.Skills_Required_To_Build);
        Dismantle_Verb = new Verb(prototype.Dismantle_Verb);
        Tools_Required_To_Build = Helper.Clone_Dictionary(prototype.Tools_Required_To_Build);
        Tools_Required_To_Dismantle = Helper.Clone_Dictionary(prototype.Tools_Required_To_Dismantle);
        Completed = HP == MAX_HP;
        Build_Menu_Tab = prototype.Build_Menu_Tab;
        Preview = is_preview;
        Harvest_Speed = prototype.Harvest_Speed;
        After_Harvest_Prototype = prototype.After_Harvest_Prototype;
        Harvest_Progress = 0.0f;
        Harvest_Drops = Helper.Clone_Dictionary(prototype.Harvest_Drops);
        Skills_Required_To_Harvest = Helper.Clone_Dictionary(prototype.Skills_Required_To_Harvest);
        Tools_Required_To_Harvest = Helper.Clone_Dictionary(prototype.Tools_Required_To_Harvest);
        Harvest_Verb = new Verb(prototype.Harvest_Verb);
        Data = new Dictionary<string, object>();
        Update_Action = prototype.Update_Action;
        update_cooldown = Update_Action != null ? (RNG.Instance.Next(0, 100) * 0.01f) * UPDATE_INTERVAL : 0.0f;
        Persistent_Data = Persistent_Data != null ? Persistent_Data : new Dictionary<string, object>();
        Create_Action = prototype.Create_Action;
        last_connections = new ConnectionData(Connections);
        Connections = new ConnectionData(prototype.Connections);
        Base_Support = prototype.Base_Support;

        if (changed_prefab) {
            Change_Prefab();
        }
        GameObject.SetActive(!Inactive_GameObject);
        if (update_material) {
            Update_Material();
            Update_Model();
        }

        if(Base_Pilar_Support && !Connections.Bottom) {
            Base_Pilar_Support = false;
            Map.Instance.Remove_Base_Pilar_Support(this);
        }
    }

    public void Rotate(float degrees_x, float degrees_y, float degrees_z)
    {
        GameObject.transform.Rotate(new Vector3(degrees_x, degrees_y, degrees_z));
    }

    private new void Update_Material()
    {
        if (Use_3DModel || Inactive_GameObject) {
            return;
        }
        base.Update_Material();
        crack_cube.SetActive(false);
        if (Completed && Relative_HP < 1.0f) {
            crack_cube.SetActive(true);
            string material_name = CRACK_MATERIALS[Mathf.RoundToInt((CRACK_MATERIALS.Length - 1) * (1.0f - Relative_HP))];
            MeshRenderer renderer = crack_cube.GetComponentInChildren<MeshRenderer>();
            if (!renderer.material.name.StartsWith(material_name)) {
                renderer.material = MaterialManager.Instance.Get(material_name, MaterialManager.MaterialType.Block);
            }
        }
        if((Preview || !Completed) && !MeshRenderer.material.name.StartsWith(TRANSPARENT_MATERIAL_PREFIX) && MaterialManager.Instance.Has(TRANSPARENT_MATERIAL_PREFIX + Material, MaterialManager.MaterialType.Block)) {
            MeshRenderer.material = MaterialManager.Instance.Get(TRANSPARENT_MATERIAL_PREFIX + Material, MaterialManager.MaterialType.Block);
        } else if (!Preview && Completed && MeshRenderer.material.name.StartsWith(TRANSPARENT_MATERIAL_PREFIX)) {
            MeshRenderer.material = MaterialManager.Instance.Get(Material, MaterialManager.MaterialType.Block);
        }
    }

    private new void Change_Prefab()
    {
        base.Change_Prefab();
        foreach (Transform transform in GameObject.transform) {
            if (transform.name == "CrackCube") {
                crack_cube = transform.gameObject;
            }
        }
        GameObject.name = string.Format("{0}#{1}_{2}", GAME_OBJECT_NAME_PREFIX, Id, Coordinates.Parse_Text(true, false));
    }

    private new void Update_Model()
    {
        base.Update_Model();
        if (!Use_3DModel) {
            return;
        }
        GameObject.name = string.Format("{0}#{1}_{2}", GAME_OBJECT_NAME_PREFIX, Id, Coordinates.Parse_Text(true, false));
    }

    public bool Deal_Damage(float amount, bool dismantle)
    {
        HP = Mathf.Max(0.0f, HP - amount);
        if (dismantle) {
            HP_Dismantled += amount;
        }
        bool broke = HP == 0.0f;
        if (broke) {
            //TODO: 0.95f
            if (Completed && HP_Dismantled >= 0.95f * MAX_HP && Dismantle_Drops.Count != 0) {
                ItemPile pile = new ItemPile(Position, ItemPile.Prototype, Map.Instance.Entity_Container, new Inventory(Dismantle_Drops));
            } else if(!Completed && HP_Dismantled >= 0.95f * HP_Built && Materials_Required_To_Build.Count != 0) {
                ItemPile pile = new ItemPile(Position, ItemPile.Prototype, Map.Instance.Entity_Container, new Inventory(Materials_Required_To_Build));
            }
            Change_To(BlockPrototypes.Instance.Air);
            Update_Structural_Integrity();
        }
        Update_Material();
        return broke;
    }

    public bool RepairOrBuild(float amount)
    {
        Harvest_Progress = 0.0f;
        HP = Mathf.Min(MAX_HP, HP + amount);
        HP_Dismantled = Mathf.Max(0.0f, HP_Dismantled - amount);
        if(!Completed) {
            HP_Built = Mathf.Min(HP, HP_Built + amount);
            if (HP == MAX_HP) {
                Completed = true;
            }
        }
        Update_Material();
        return HP == MAX_HP;
    }

    public bool Harvest(float amount, Inventory inventory)
    {
        if (!Harvestable) {
            return false;
        }
        Harvest_Progress = Mathf.Min(MAX_HP, Harvest_Progress + amount);
        bool harvested = Harvest_Progress == MAX_HP;
        if (harvested) {
            Block prototype = BlockPrototypes.Instance.Get(string.IsNullOrEmpty(After_Harvest_Prototype) ? BlockPrototypes.AIR_INTERNAL_NAME : After_Harvest_Prototype);
            Inventory ground_drops = new Inventory();
            Inventory added_items = new Inventory();
            foreach(KeyValuePair<string, int> drop in Harvest_Drops) {
                for(int i = 0; i < drop.Value; i++) {
                    Item item = ItemPrototypes.Instance.Get_Item(drop.Key);
                    if (inventory.Can_Fit(item)) {
                        inventory.Add(item);
                        added_items.Add(item);
                    } else {
                        ground_drops.Add(item);
                    }
                }
            }
            if (inventory == Player.Current.Inventory && !added_items.Is_Empty) {
                FloatingMessageManager.Instance.Show(added_items.Parse_Text(true));
            }
            Change_To(prototype);
            if (!ground_drops.Is_Empty) {
                Block closest = Map.Instance.Find_Closest_Passable_Block(Coordinates);
                if(closest != null) {
                    ItemPile pile = new ItemPile(closest.Position, ItemPile.Prototype, Map.Instance.Entity_Container, inventory);
                } else {
                    CustomLogger.Instance.Warning("Block not found");
                }
            }
        }
        return harvested;
    }

    public void Update(float delta_time)
    {
        if(Update_Action == null || !Active) {
            return;
        }
        update_cooldown -= delta_time;
        if(update_cooldown > 0.0f) {
            return;
        }
        update_cooldown += UPDATE_INTERVAL;
        delta_time = update_cooldown;
        Update_Action(delta_time, this);
    }

    public static Block Load(BlockSaveData data, GameObject container)
    {
        if(current_id <= data.Id) {
            current_id = data.Id + 1;
        }
        Block prototype = BlockPrototypes.Instance.Get(data.Internal_Name);
        Block block = new Block(new Coordinates(data.Coordinates), prototype, container);
        if(data.Rotation != null && (data.Rotation.X != 0 || data.Rotation.Y != 0 || data.Rotation.Z != 0)) {
            block.Rotate(data.Rotation.X, data.Rotation.Y, data.Rotation.Z);
        }
        block.Base_Pilar_Support = data.Base_Pilar_Support;
        return block;
    }

    public BlockSaveData Save_Data
    {
        get {
            return new BlockSaveData {
                Id = Id,
                Coordinates = Coordinates.Save_Data,
                Internal_Name = Internal_Name,
                Rotation = new CoordinatesSaveData {
                    X = (int)GameObject.transform.rotation.eulerAngles.x,
                    Y = (int)GameObject.transform.rotation.eulerAngles.y,
                    Z = (int)GameObject.transform.rotation.eulerAngles.z
                },
                Base_Pilar_Support = Base_Pilar_Support
            };
        }
    }

    public void Debug_Tag(string tag)
    {
        if(Name.StartsWith(string.Format("{0}_", tag))) {
            return;
        }
        CustomLogger.Instance.Debug(string.Format("{0} -> {1}", GameObject.name, tag));
        Name = string.Format("{0}_{1}", tag, Name);
    }
    
    public void Update_Structural_Integrity()
    {
        Start_Stopwatch_Logging("STUCTURAL INTEGRITY");
        Start_Stopwatch("Update_Structural_Integrity");
        update_structural_integrity_calls = 0;
        List<Block> connected_blocks = new List<Block>();
        foreach (ConnectionData.Direction direction in Connections.Lost_Connetions(last_connections)) {
            Start_Stopwatch("Map.Instance.Get_Block_At");
            Block block = Map.Instance.Get_Block_At(Coordinates.Shift(ConnectionData.To_Coordinate_Delta(direction)));
            Stop_Stopwatch("Map.Instance.Get_Block_At");
            if (block == null || block.Is_Air || block.Base_Support || connected_blocks.Contains(block)) {
                continue;
            }
            List<Block> blocks = new List<Block>();
            bool has_support = false;
            Stop_Stopwatch("Update_Structural_Integrity");
            Check_Connected_Blocks(block, ref has_support, ref blocks, connected_blocks);
            Start_Stopwatch("Update_Structural_Integrity");
            if (!has_support) {
                foreach(Block b in blocks) {
                    b.Change_To(BlockPrototypes.Instance.Air);
                }
            } else {
                foreach (Block b in blocks) {
                    connected_blocks.Add(b);
                }
            }
        }
        Stop_Stopwatch("Update_Structural_Integrity");
        Print_Stopwatches();
    }

    private void Check_Connected_Blocks(Block block, ref bool has_support, ref List<Block> blocks, List<Block> connected_blocks)
    {
        Start_Stopwatch("Check_Connected_Blocks");
        blocks.Add(block);
        if (connected_blocks.Contains(block)) {
            has_support = true;
            return;
        }
        foreach (ConnectionData.Direction direction in block.Connections.All) {
            Start_Stopwatch("Map.Instance.Get_Block_At");
            Block b = Map.Instance.Get_Block_At(block.Coordinates.Shift(ConnectionData.To_Coordinate_Delta(direction)));
            Stop_Stopwatch("Map.Instance.Get_Block_At");
            if (b == null || b.Is_Air) {
                continue;
            }
            if (b.Base_Support || (direction == ConnectionData.Direction.Bottom && b.Base_Pilar_Support)) {
                has_support = true;
                return;
            }
            Stop_Stopwatch("Check_Connected_Blocks");
            Check_Connected_Blocks_Recursive(b, 0, ref has_support, ref blocks, connected_blocks);
            Start_Stopwatch("Check_Connected_Blocks");
        }
        Stop_Stopwatch("Check_Connected_Blocks");
    }

    private void Check_Connected_Blocks_Recursive(Block block, int range, ref bool has_support, ref List<Block> blocks, List<Block> connected_blocks)
    {
        Start_Stopwatch("Check_Connected_Blocks_Recursive");
        if (has_support) {
            return;
        }
        update_structural_integrity_calls++;
        blocks.Add(block);
        range++;
        if(range == UPDATE_STRUCTURAL_INTERGRITY_RANGE || connected_blocks.Contains(block) || update_structural_integrity_calls == UPDATE_STRUCTURAL_INTERGRITY_MAX_CALLS) {
            has_support = true;
            return;
        }
        foreach (ConnectionData.Direction direction in block.Connections.All) {
            Start_Stopwatch("Map.Instance.Get_Block_At");
            Block b = Map.Instance.Get_Block_At(block.Coordinates.Shift(ConnectionData.To_Coordinate_Delta(direction)));
            Stop_Stopwatch("Map.Instance.Get_Block_At");
            if (b == null || b.Is_Air || blocks.Contains(b)) {
                continue;
            }
            if (b.Base_Support || (direction == ConnectionData.Direction.Bottom && b.Base_Pilar_Support)) {
                has_support = true;
                return;
            }
            Check_Connected_Blocks_Recursive(b, range, ref has_support, ref blocks, connected_blocks);
        }
        Stop_Stopwatch("Check_Connected_Blocks_Recursive");
    }
    
    private void Start_Stopwatch_Logging(string title)
    {
        if (!Log_Diagnostics) {
            return;
        }
        watch_title = title;
        CustomLogger.Instance.Debug(string.Format("--- {0} ---", watch_title));
        watches = new Dictionary<string, Stopwatch>();
    }

    private void Start_Stopwatch(string name)
    {
        if (!Log_Diagnostics) {
            return;
        }
        if (!watches.ContainsKey(name)) {
            watches.Add(name, Stopwatch.StartNew());
        } else {
            watches[name].Start();
        }
    }

    private void Stop_Stopwatch(string name)
    {
        if (!Log_Diagnostics) {
            return;
        }
        if (watches.ContainsKey(name)) {
            watches[name].Stop();
        }
    }

    private void Print_Stopwatches()
    {
        if (!Log_Diagnostics) {
            return;
        }
        foreach (KeyValuePair<string, Stopwatch> pair in watches) {
            pair.Value.Stop();
        }
        long total = 0;
        foreach (KeyValuePair<string, Stopwatch> pair in watches) {
            CustomLogger.Instance.Debug(string.Format("{0}: {1}ms", pair.Key, pair.Value.ElapsedMilliseconds));
            total += pair.Value.ElapsedMilliseconds;
        }
        watches.Clear();
        CustomLogger.Instance.Debug(string.Format("TOTAL: {0}ms", total));
        CustomLogger.Instance.Debug(string.Format("--- {0} ---", watch_title));
        watch_title = null;
    }

    public new void Delete()
    {
        base.Delete();
        Map.Instance.Remove_Block(this);
    }

    public static void Reset_Current_Id()
    {
        current_id = 0;
    }

    public override string ToString()
    {
        return string.Format("{0} block #{1} {2}", Internal_Name, Id, Coordinates.Parse_Text(true, true));
    }

    private PrototypeData Prototype_Data
    {
        get {
            return string.IsNullOrEmpty(Model_Name) ?
                new PrototypeData(Prefab_Name, Material, Material_Type) :
                new PrototypeData(Model_Name, !Passable);
        }
    }
}

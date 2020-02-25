using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class Map {
    private static Map instance;

    public int Size_X { get; private set; }
    public int Size_Y { get; private set; }
    public int Size_Z { get; private set; }
    public int Chunk_Size_X { get; private set; }
    public int Chunk_Size_Z { get; private set; }
    public GameObject Entity_Container { get; private set; }
    public GameObject Block_Container { get; private set; }
    public bool Generating { get; private set; }
    public int Rendering_Distance { get; private set; }

    private GameObject game_object;
    private List<Block> blocks;
    private List<Chunk> chunks;
    private List<Chunk> active_chunks;
    private List<Entity> entities;
    private List<Entity> entities_to_be_added;
    private List<Entity> entities_to_be_removed;
    private List<Block> blocks_to_be_added;
    private List<Block> blocks_to_be_removed;
    private Block player_spawn;
    private bool active;

    private int chunk_index_x = 0;
    private int chunk_index_z = 0;

    private Map()
    {
        game_object = GameObject.Find("Map");
        Block_Container = GameObject.Find("Map/Blocks");
        Entity_Container = GameObject.Find("Map/Entitys");
        blocks = new List<Block>();
        chunks = new List<Chunk>();
        active_chunks = new List<Chunk>();
        entities = new List<Entity>();
        entities_to_be_added = new List<Entity>();
        entities_to_be_removed = new List<Entity>();
        blocks_to_be_added = new List<Block>();
        blocks_to_be_removed = new List<Block>();
        active = false;
        Rendering_Distance = 30;
    }

    public static Map Instance
    {
        get {
            if(instance == null) {
                instance = new Map();
            }
            return instance;
        }
    }


    public List<Player> Players
    {
        get {
            return entities.Where(x => x is Player).Select(x => x as Player).ToList();
        }
    }

    public List<Entity> Entities
    {
        get {
            return entities.Select(x => x).ToList();
        }
    }

    public void Generate_New(int chunk_size_x, int size_y, int chunk_size_z)
    {
        Delete();
        chunk_index_x = 0;
        chunk_index_z = 0;

        Size_X = chunk_size_x * Chunk.SIZE_X;
        Size_Y = size_y;
        Size_Z = chunk_size_z * Chunk.SIZE_Z;
        Chunk_Size_X = chunk_size_x;
        Chunk_Size_Z = chunk_size_z;

        Generating = true;
        ProgressBarManager.Instance.Active = true;
        Update_Progress();
    }

    public void Update(float delta_time)
    {
        if (Generating) {
            Generate();
            return;
        }
        if (!active) {
            return;
        }
        Stopwatch stopwatch = Stopwatch.StartNew();
        foreach (Entity entity in entities) {
            if (entity is Player) {
                (entity as Player).Update(delta_time + stopwatch.ElapsedMilliseconds * 0.001f);
                if(entity.Position.y < -100.0f) {
                    entity.Position = player_spawn.Position;
                }
            } else if (entity is Mob) {
                (entity as Mob).Update(delta_time + stopwatch.ElapsedMilliseconds * 0.001f);
            } else {
                entity.Update(delta_time + stopwatch.ElapsedMilliseconds * 0.001f);
            }
        }

        foreach(Entity entity in entities_to_be_added) {
            entities.Add(entity);
        }
        entities_to_be_added.Clear();

        foreach(Entity entity in entities_to_be_removed) {
            entities.Remove(entity);
            if(InspectorManager.Instance.Target == entity) {
                InspectorManager.Instance.Target = null;
            }
        }
        entities_to_be_removed.Clear();

        foreach (Block block in blocks_to_be_added) {
            blocks.Add(block);
        }
        blocks_to_be_added.Clear();

        foreach (Block block in blocks_to_be_removed) {
            blocks.Remove(block);
            if (InspectorManager.Instance.Target == block) {
                InspectorManager.Instance.Target = null;
            }
        }
        blocks_to_be_removed.Clear();

        Load_Chunks();
    }

    private void Generate()
    {
        Chunk chunk = new Chunk(chunk_index_x, chunk_index_z);

        int x_start = chunk.X * Chunk.SIZE_X;
        int z_start = chunk.Z * Chunk.SIZE_Z;

        for (int x = x_start; x < x_start + Chunk.SIZE_X; x++) {
            for(int y = 0; y < Size_Y; y++) {
                for(int z = z_start; z < z_start + Chunk.SIZE_Z; z++) {
                    Block block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get(y > Size_Y / 2 ? BlockPrototypes.AIR_INTERNAL_NAME : "rock"), chunk.GameObject);
                    block.Chunk = chunk;
                    chunk.Blocks.Add(block);
                    blocks.Add(block);
                }
            }
        }
        chunk.Active = false;

        chunks.Add(chunk);
        chunk_index_x++;
        if(chunk_index_x == Chunk_Size_X) {
            chunk_index_x = 0;
            chunk_index_z++;
        }

        if(chunks.Count == Chunk_Size_X * Chunk_Size_Z) {
            Finish_Generation();
        } else {
            Update_Progress();
        }
    }

    private void Finish_Generation()
    {
        player_spawn = blocks.OrderBy(x => x.Coordinates.Y).FirstOrDefault(x => x.Coordinates.X == Size_X / 2 && x.Coordinates.Z == Size_Z / 2 && x.Passable);
        if (player_spawn == null) {
            CustomLogger.Instance.Error("Player spawn not found");
        }

        Player player = new Player(player_spawn.Position, Player.Prototype, Entity_Container);
        entities.Add(player);
        Load_Chunks();

        active = true;
        Generating = false;
        CameraManager.Instance.Reset();
        ProgressBarManager.Instance.Active = false;
        
        /*foreach(Chunk c in chunks) {
            Block b = Get_Block_At(new Coordinates((int)c.Center.x, Size_Y - 1, (int)c.Center.z));
            Mob mob = new Mob(b.Position, Mob.Dummy_Prototype, Entity_Container);
            mob.GameObject.name = c.Id.ToString();
        }*/
    }

    private void Update_Progress()
    {
        float progress = (float)chunks.Count / ((float)Chunk_Size_X * (float)Chunk_Size_Z);
        ProgressBarManager.Instance.Show(string.Format("Generating map... {0}%", Helper.Float_To_String(100.0f * progress, 1)), progress);
    }

    private void Load_Chunks()
    {
        //TODO: Optimize
        active_chunks.Clear();
        foreach(Chunk chunk in chunks) {
            bool render = Vector3.Distance(chunk.Center, new Vector3(Player.Current.GameObject.transform.position.x, 0.0f, Player.Current.GameObject.transform.position.z)) <= Rendering_Distance;
            if(chunk.Active && !render) {
                chunk.Active = false;
            } else if(!chunk.Active && render) {
                chunk.Active = true;
            }
        }
    }

    public void Add_Entity(Entity entity)
    {
        if(!entities_to_be_added.Exists(x => x.Id == entity.Id)) {
            entities_to_be_added.Add(entity);
        }
    }

    public void Remove_Entity(Entity entity)
    {
        if (!entities_to_be_removed.Exists(x => x.Id == entity.Id)) {
            entities_to_be_removed.Add(entity);
        }
    }

    public void Add_Block(Block block)
    {
        if (!active) {
            blocks.Add(block);
            return;
        }
        if (!blocks_to_be_added.Exists(x => x.Id == block.Id)) {
            blocks_to_be_added.Add(block);
        }
    }

    public void Remove_Block(Block block)
    {
        if (!active) {
            blocks.Remove(block);
            return;
        }
        if (!blocks_to_be_removed.Exists(x => x.Id == block.Id)) {
            blocks_to_be_removed.Add(block);
        }
    }

    public Entity Get_Entity(long entity_id)
    {
        return entities.FirstOrDefault(x => x.Id == entity_id);
    }

    public Block Get_Block(long block_id)
    {
        return blocks.FirstOrDefault(x => x.Id == block_id);
    }

    public Block Get_Block_At(Coordinates coordinates)
    {
        return blocks.FirstOrDefault(x => x.Coordinates.Equals(coordinates));
    }

    //TODO: this
    public Block Find_Closest_Passable_Block(Coordinates coordinates)
    {
        return blocks.Where(x => x.Coordinates.X == coordinates.X && x.Coordinates.Z == coordinates.Z && x.Passable).OrderBy(x => x.Coordinates.Y).FirstOrDefault();
    }

    public bool Active
    {
        get {
            return active;
        }
    }

    private void Delete()
    {
        foreach(Block block in blocks) {
            block.Delete();
        }
        blocks.Clear();
        foreach(Chunk chunk in chunks) {
            chunk.Delete();
        }
        chunks.Clear();
        active_chunks.Clear();

        foreach (Entity entity in entities) {
            if(entity is Player) {
                (entity as Player).Delete();
            } else if(entity is Mob) {
                (entity as Mob).Delete();
            } else {
                entity.Delete();
            }
        }
        foreach (Entity entity in entities_to_be_added) {
            if (entity is Player) {
                (entity as Player).Delete();
            } else if (entity is Mob) {
                (entity as Mob).Delete();
            } else {
                entity.Delete();
            }
        }
        foreach (Entity entity in entities_to_be_removed) {
            if (entity is Player) {
                (entity as Player).Delete();
            } else if (entity is Mob) {
                (entity as Mob).Delete();
            } else {
                entity.Delete();
            }
        }
        entities.Clear();
        entities_to_be_added.Clear();
        entities_to_be_removed.Clear();

        active = false;
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class Map {
    private static Map instance;

    private static bool PRINT_DIAGNOSTICS = true;
    
    public int Size_Y { get; private set; }
    public int Chunk_Size_X { get; private set; }
    public int Chunk_Size_Z { get; private set; }
    public int Initial_Chunk_Size_X { get; private set; }
    public int Initial_Chunk_Size_Z { get; private set; }
    public GameObject Entity_Container { get; private set; }
    public GameObject Block_Container { get; private set; }
    public bool Generating { get; private set; }
    public int Rendering_Distance { get; private set; }
    public bool Limited_Generation { get; private set; }
    public bool Simple_Elevation { get; private set; }

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

    private int chunk_index_x;
    private int chunk_index_z;
    private int generation_loop;
    private int second_loop_count;
    private int third_loop_count;
    private Stopwatch stopwatch;
    private float loop_time_1;
    private float loop_time_2;
    private float loop_time_3;

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
        chunk_index_x = 0;
        chunk_index_z = 0;
        generation_loop = 0;
        second_loop_count = 0;
        third_loop_count = 0;
        stopwatch = null;
        loop_time_1 = 0.0f;
        loop_time_2 = 0.0f;
        loop_time_3 = 0.0f;
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

    public void Generate_New(int chunk_size_x, int size_y, int chunk_size_z, int initial_chunk_size_x, int initial_chunk_size_z, bool limited_generation, bool simple_elevation)
    {
        if (PRINT_DIAGNOSTICS) {
            stopwatch = Stopwatch.StartNew();
        }
        Delete();
        chunk_index_x = 0;
        chunk_index_z = 0;
        generation_loop = 0;
        second_loop_count = 0;
        third_loop_count = 0;
        loop_time_1 = 0.0f;
        loop_time_2 = 0.0f;
        loop_time_3 = 0.0f;

        Size_Y = size_y;
        Chunk_Size_X = chunk_size_x;
        Chunk_Size_Z = chunk_size_z;
        Initial_Chunk_Size_X = initial_chunk_size_x;
        Initial_Chunk_Size_Z = initial_chunk_size_z;
        Limited_Generation = limited_generation;
        Simple_Elevation = simple_elevation;

        Generating = true;
        ProgressBarManager.Instance.Active = true;
        Update_Progress();
    }

    public void Update(float delta_time)
    {
        if (Generating) {
            if(generation_loop == 0) {
                Generate_First_Loop();
            } else if (generation_loop == 1) {
                Generate_Second_Loop();
            } else {
                Generate_Third_Loop();
            }
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

        foreach(Chunk chunck in active_chunks) {
            foreach(Block block in chunck.Blocks) {
                block.Update(delta_time + stopwatch.ElapsedMilliseconds * 0.001f);
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

        Generate_Chunks();
        Load_Chunks();
    }

    private void Generate_First_Loop()
    {
        Chunk chunk = new Chunk(chunk_index_x, chunk_index_z);
        chunk.Generate_First_Loop(Size_Y, Get_Chuck_Elevation(chunk), Simple_Elevation);
        foreach(Block block in chunk.Blocks) {
            blocks.Add(block);
        }
        chunk.Active = false;

        chunks.Add(chunk);
        chunk_index_x++;
        if(chunk_index_x == Initial_Chunk_Size_X) {
            chunk_index_x = 0;
            chunk_index_z++;
        }

        if(chunks.Count == Initial_Chunk_Size_X * Initial_Chunk_Size_Z) {
            chunk_index_x = 0;
            chunk_index_z = 0;
            generation_loop = 1;
            if (PRINT_DIAGNOSTICS) {
                loop_time_1 = stopwatch.ElapsedMilliseconds * 0.001f;
                stopwatch = Stopwatch.StartNew();
            }
        } else {
            Update_Progress();
        }
    }

    private void Generate_Second_Loop()
    {
        Chunk chunk = chunks.First(x => x.X == chunk_index_x && x.Z == chunk_index_z);
        chunk.Generate_Second_Loop();

        second_loop_count++;
        chunk_index_x++;
        if (chunk_index_x == Initial_Chunk_Size_X) {
            chunk_index_x = 0;
            chunk_index_z++;
        }

        if (second_loop_count == Initial_Chunk_Size_X * Initial_Chunk_Size_Z) {
            chunk_index_x = 0;
            chunk_index_z = 0;
            generation_loop = 2;
            if (PRINT_DIAGNOSTICS) {
                loop_time_2 = stopwatch.ElapsedMilliseconds * 0.001f;
                stopwatch = Stopwatch.StartNew();
            }
        } else {
            Update_Progress();
        }
    }

    private void Generate_Third_Loop()
    {
        Chunk chunk = chunks.First(x => x.X == chunk_index_x && x.Z == chunk_index_z);
        chunk.Generate_Third_Loop();

        third_loop_count++;
        chunk_index_x++;
        if (chunk_index_x == Initial_Chunk_Size_X) {
            chunk_index_x = 0;
            chunk_index_z++;
        }

        if (third_loop_count == Initial_Chunk_Size_X * Initial_Chunk_Size_Z) {
            if (PRINT_DIAGNOSTICS) {
                loop_time_3 = stopwatch.ElapsedMilliseconds * 0.001f;
                stopwatch.Stop();
            }
            Finish_Generation();
        } else {
            Update_Progress();
        }
    }

    private void Finish_Generation()
    {
        foreach(Chunk chunk in chunks) {
            chunk.End_Generation();
        }
        player_spawn = blocks.OrderBy(x => x.Coordinates.Y).FirstOrDefault(x => x.Coordinates.X == (Initial_Chunk_Size_X * Chunk.SIZE_X) / 2 && x.Coordinates.Z == (Initial_Chunk_Size_Z * Chunk.SIZE_Z) / 2 && x.Passable);
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
        PlayerGUIManager.Instance.Active = true;

        if (PRINT_DIAGNOSTICS) {
            CustomLogger.Instance.Debug(string.Format("Map gen loop 1 in: {0}s", loop_time_1));
            CustomLogger.Instance.Debug(string.Format("Map gen loop 2 in: {0}s", loop_time_2));
            CustomLogger.Instance.Debug(string.Format("Map gen loop 3 in: {0}s", loop_time_3));
        }
    }

    private void Update_Progress()
    {
        float loop_1_current = chunks.Count;
        float loop_2_current = second_loop_count;
        float loop_3_current = third_loop_count;
        float chunk_count = (float)Initial_Chunk_Size_X * (float)Initial_Chunk_Size_Z;
        float progress = (loop_1_current + loop_2_current + loop_3_current) / (chunk_count * 3.0f);
        string message = "Generating map";
        switch (generation_loop) {
            case 0:
                message = "Generating terrain";
                break;
            case 1:
                message = "Fine tuning terrain";
                break;
            case 2:
                message = "Fine tuning details";
                break;
        }
        ProgressBarManager.Instance.Show(string.Format("{0}... {1}%", message, Helper.Float_To_String(100.0f * progress, 1)), progress);
    }

    private void Generate_Chunks()
    {
        if (Limited_Generation) {
            return;
        }
        int player_x = (int)Player.Current.Position.x / Chunk.SIZE_X;
        int player_z = (int)Player.Current.Position.z / Chunk.SIZE_Z;
        for (int x = player_x - (Chunk_Size_X / 2); x < player_x + (Chunk_Size_X / 2); x++) {
            for (int z = player_z - (Chunk_Size_Z / 2); z < player_z + (Chunk_Size_Z / 2); z++) {
                Chunk chunk = chunks.FirstOrDefault(c => c.X == x && c.Z == z);
                if(chunk != null) {
                    continue;
                }
                chunk = new Chunk(x, z);

                chunk.Generate_First_Loop(Size_Y, Get_Chuck_Elevation(chunk), Simple_Elevation);
                chunk.Generate_Second_Loop();
                chunk.Generate_Third_Loop();

                chunks.Add(chunk);
            }
        }
    }

    private int Get_Chuck_Elevation(Chunk chunk)
    {
        int size_elevation = Size_Y / 2;
        if (Simple_Elevation) {
            return size_elevation;
        }
        float elevation_total = 0.0f;
        float elevation_count = 0.0f;
        foreach (Chunk c in chunks) {
            if ((c.X == chunk.X - 1 || c.X == chunk.X + 1) && (c.Z == chunk.Z - 1 || c.Z == chunk.Z + 1)) {
                elevation_total += c.Average_Elevation;
                elevation_total += 1.0f;
            }
        }
        int base_elevation = elevation_count != 0 ? Mathf.RoundToInt(elevation_total / elevation_count) : size_elevation;
        return base_elevation + RNG.Instance.Next(-1, 1);
    }

    private void Load_Chunks()
    {
        //TODO: Optimize
        foreach(Chunk chunk in chunks) {
            bool render = Vector3.Distance(chunk.Center, new Vector3(Player.Current.GameObject.transform.position.x, 0.0f, Player.Current.GameObject.transform.position.z)) <= Rendering_Distance;
            if(chunk.Active && !render) {
                chunk.Active = false;
                if (active_chunks.Contains(chunk)) {
                    active_chunks.Remove(chunk);
                }
            } else if(!chunk.Active && render) {
                chunk.Active = true;
                if (!active_chunks.Contains(chunk)) {
                    active_chunks.Add(chunk);
                }
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

    public List<Block> Get_Blocks_In_Chuck_And_Adjacent_Chunks(Chunk chunk)
    {
        List<Chunk> list = chunks.Where(x =>
            (x.X == chunk.X && x.Z == chunk.Z) ||
            (x.X + 1 == chunk.X && x.Z == chunk.Z) ||
            (x.X - 1 == chunk.X && x.Z == chunk.Z) ||
            (x.X == chunk.X && x.Z + 1 == chunk.Z) ||
            (x.X == chunk.X && x.Z - 1 == chunk.Z)
        ).ToList();
        //TODO: linq
        List<Block> b = new List<Block>();
        foreach(Chunk c in list) {
            b.AddRange(c.Blocks);
        }
        return b;
    }

    public List<Chunk> Get_Adjacent_Chunks(Chunk chunk)
    {
        return chunks.Where(x =>
            (x.X == chunk.X && x.Z == chunk.Z) ||
            (x.X + 1 == chunk.X && x.Z == chunk.Z) ||
            (x.X - 1 == chunk.X && x.Z == chunk.Z) ||
            (x.X == chunk.X && x.Z + 1 == chunk.Z) ||
            (x.X == chunk.X && x.Z - 1 == chunk.Z)
        ).ToList(); ;
    }

    //TODO: this
    public Block Find_Closest_Passable_Block(Coordinates coordinates)
    {
        return blocks.Where(x => x.Coordinates.X == coordinates.X && x.Coordinates.Z == coordinates.Z && x.Passable).OrderBy(x => x.Coordinates.Y).FirstOrDefault();
    }

    public static List<Block> Get_Adjacent(Block block, List<Block> blocks)
    {
        return blocks.Where(x =>
            (x.Coordinates.X == block.Coordinates.X - 1 && x.Coordinates.Y == block.Coordinates.Y && x.Coordinates.Z == block.Coordinates.Z) ||
            (x.Coordinates.X == block.Coordinates.X + 1 && x.Coordinates.Y == block.Coordinates.Y && x.Coordinates.Z == block.Coordinates.Z) ||
            (x.Coordinates.X == block.Coordinates.X && x.Coordinates.Y - 1 == block.Coordinates.Y && x.Coordinates.Z == block.Coordinates.Z) ||
            (x.Coordinates.X == block.Coordinates.X && x.Coordinates.Y + 1 == block.Coordinates.Y && x.Coordinates.Z == block.Coordinates.Z) ||
            (x.Coordinates.X == block.Coordinates.X && x.Coordinates.Y == block.Coordinates.Y && x.Coordinates.Z - 1 == block.Coordinates.Z) ||
            (x.Coordinates.X == block.Coordinates.X && x.Coordinates.Y == block.Coordinates.Y && x.Coordinates.Z + 1 == block.Coordinates.Z)
        ).ToList();
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

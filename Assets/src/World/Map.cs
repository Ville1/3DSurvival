using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class Map {
    private static Map instance;

    public int Size_X { get; private set; }
    public int Size_Y { get; private set; }
    public int Size_Z { get; private set; }
    public GameObject Entity_Container { get; private set; }
    public GameObject Block_Container { get; private set; }

    private GameObject game_object;
    private List<Block> blocks;
    private List<Entity> entities;
    private List<Entity> entities_to_be_added;
    private List<Entity> entities_to_be_removed;
    private List<Block> blocks_to_be_added;
    private List<Block> blocks_to_be_removed;
    private Block player_spawn;
    private bool active;

    private Map()
    {
        game_object = GameObject.Find("Map");
        Block_Container = GameObject.Find("Map/Blocks");
        Entity_Container = GameObject.Find("Map/Entitys");
        blocks = new List<Block>();
        entities = new List<Entity>();
        entities_to_be_added = new List<Entity>();
        entities_to_be_removed = new List<Entity>();
        blocks_to_be_added = new List<Block>();
        blocks_to_be_removed = new List<Block>();
        active = false;
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

    public void Generate_New(int size_x, int size_y, int size_z)
    {
        Delete();

        Size_X = size_x;
        Size_Y = size_y;
        Size_Z = size_z;
        for(int x = 0; x < Size_X; x++) {
            for(int y = 0; y < Size_Y; y++) {
                for (int z = 0; z < Size_Z; z++) {
                    Block block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get(y < Size_Y / 2 ? "rock" : BlockPrototypes.AIR_INTERNAL_NAME), Block_Container);
                }
            }
        }

        player_spawn = blocks.OrderBy(x => x.Coordinates.Y).FirstOrDefault(x => x.Coordinates.X == Size_X / 2 && x.Coordinates.Z == Size_Z / 2 && x.Passable);
        if(player_spawn == null) {
            CustomLogger.Instance.Error("Player spawn not found");
        }
        
        Player player = new Player(player_spawn.Position, Player.Prototype, Entity_Container);
        entities.Add(player);

        active = true;
        CameraManager.Instance.Reset();
    }

    public void Update(float delta_time)
    {
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

        foreach(Entity entity in entities) {
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

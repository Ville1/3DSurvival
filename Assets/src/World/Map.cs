﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class Map {
    private static Map instance;

    public int Size_X { get; private set; }
    public int Size_Y { get; private set; }
    public int Size_Z { get; private set; }

    private GameObject game_object;
    private GameObject block_container;
    private GameObject entity_container;
    private List<Block> blocks;
    private List<Entity> entities;
    private List<Entity> entities_to_be_added;
    private List<Entity> entities_to_be_removed;
    private Block player_spawn;
    private bool active;

    private Map()
    {
        game_object = GameObject.Find("Map");
        block_container = GameObject.Find("Map/Blocks");
        entity_container = GameObject.Find("Map/Entitys");
        blocks = new List<Block>();
        entities = new List<Entity>();
        entities_to_be_added = new List<Entity>();
        entities_to_be_removed = new List<Entity>();
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
                    Block block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get(y < Size_Y / 2 ? "rock" : BlockPrototypes.AIR_INTERNAL_NAME), block_container);
                    blocks.Add(block);
                }
            }
        }

        player_spawn = blocks.OrderBy(x => x.Coordinates.Y).FirstOrDefault(x => x.Coordinates.X == Size_X / 2 && x.Coordinates.Z == Size_Z / 2 && x.Passable);
        if(player_spawn == null) {
            CustomLogger.Instance.Error("Player spawn not found");
        }

        Player prototype_player = new Player("Player 1", "Player");
        Player player = new Player(player_spawn.Position, prototype_player, entity_container);
        Players.Add(player);

        active = true;
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
        }
        entities_to_be_removed.Clear();
    }

    public void Add_Entity(Entity entity)
    {
        if(entities_to_be_added.FirstOrDefault(x => x.Id == entity.Id) != null) {
            entities_to_be_added.Add(entity);
        }
    }

    public void Remove_Entity(Entity entity)
    {
        if (entities_to_be_removed.FirstOrDefault(x => x.Id == entity.Id) != null) {
            entities_to_be_removed.Add(entity);
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

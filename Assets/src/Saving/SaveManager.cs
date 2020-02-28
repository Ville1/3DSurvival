using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager
{
    private static SaveManager instance;
    
    public int Chunks_Loaded { get; private set; }

    private SaveData data;
    private string path;
    private int chunk_index;

    private SaveManager()
    {
    }

    public static SaveManager Instance
    {
        get {
            if (instance == null) {
                instance = new SaveManager();
            }
            return instance;
        }
    }

    public bool Start_Saving(string path)
    {
        try {
            this.path = path;
            data = new SaveData();
            data.Player = new PlayerSaveData();
            data.Player.Coordinates = new Coordinates(Player.Current.Position).Save_Data;
            data.Player.Spawn = Map.Instance.Player_Spawn.Coordinates.Save_Data;
            data.Player.Items = new List<ItemSaveData>();
            foreach(Item item in Player.Current.Inventory) {
                data.Player.Items.Add(item.Save_Data);
            }
            data.Chunks = new List<ChunkSaveData>();
            return true;
        }
        catch(Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            return false;
        }
    }

    public bool Start_Loading(string path)
    {
        try {
            this.path = path;
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            Chunks_Loaded = data.Chunks.Count;
            chunk_index = 0;
            return true;
        } catch (Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            return false;
        }
    }

    public void Add(ChunkSaveData chunk)
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Saving needs to be called before Add");
        } else {
            data.Chunks.Add(chunk);
        }
    }

    public Chunk Load_Next()
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Loading needs to be called before Load_Next");
            return null;
        } else {
            Chunk chunk = new Chunk(data.Chunks[chunk_index]);
            chunk.Active = false;
            chunk_index++;
            return chunk;
        }
    }

    public SaveData Data
    {
        get {
            return data;
        }
    }

    public bool Finish_Saving()
    {
        try {
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
            data = null;
            return true;
        } catch (Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            data = null;
            return false;
        }
    }

    public List<Item> Load_Items()
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Loading needs to be called before Load_Items");
            return null;
        } else {
            List<Item> items = new List<Item>();
            foreach(ItemSaveData item_data in data.Player.Items) {
                items.Add(new Item(item_data));
            }
            return items;
        }
    }

    public void Finish_Loading()
    {
        data = null;
        Chunks_Loaded = 0;
        chunk_index = 0;
    }
}

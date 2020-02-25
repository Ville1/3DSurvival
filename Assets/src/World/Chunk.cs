using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public static readonly int SIZE_X = 10;
    public static readonly int SIZE_Z = 10;

    private static long current_id = 0;

    public long Id { get; private set; }
    public int X { get; private set; }
    public int Z { get; private set; }
    public List<Block> Blocks { get; private set; }
    public GameObject GameObject { get; private set; }
    
    public Chunk(int x, int z)
    {
        Id = current_id;
        current_id++;
        X = x;
        Z = z;
        Blocks = new List<Block>();
        GameObject = new GameObject(string.Format("Chunk_({0},{1})#{2}", X, Z, Id));
        GameObject.transform.parent = Map.Instance.Block_Container.transform;
        GameObject.SetActive(true);
    }

    public void Delete()
    {
        GameObject.Destroy(GameObject);
    }

    public override string ToString()
    {
        return string.Format("Chunk ({0}, {1}) #{2}", X, Z, Id);
    }

    public bool Active
    {
        get {
            return GameObject.activeSelf;
        }
        set {
            GameObject.SetActive(value);
        }
    }
    
    public Vector3 Center
    {
        get {
            return new Vector3((X * SIZE_X) + (SIZE_X * 0.5f), 0.0f, (Z * SIZE_Z) + (SIZE_Z * 0.5f));
        }
    }
}

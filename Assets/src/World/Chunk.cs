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

    public void Generate(int size_y)
    {
        int x_start = X * SIZE_X;
        int z_start = Z * SIZE_Z;

        for (int x = x_start; x < x_start + SIZE_X; x++) {
            for (int y = 0; y < size_y; y++) {
                for (int z = z_start; z < z_start + SIZE_Z; z++) {
                    string prototype = BlockPrototypes.AIR_INTERNAL_NAME;
                    if(y < size_y / 2 - 1) {
                        prototype = "rock";
                    } else if(y == size_y / 2 - 1) {
                        prototype = "dirt";
                    } else if (y == size_y / 2) {
                        prototype = "grass";
                    } else if (y == size_y / 2 + 1 && RNG.Instance.Next(0, 9) == 0) {
                        int rand = RNG.Instance.Next(0, 100);
                        if(rand <= 35) {
                            prototype = "tall_grass";
                        } else if(rand > 25 && rand <= 85) {
                            prototype = "medium_grass";
                        } else {
                            prototype = "short_grass";
                        }
                    }
                    Block block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get(prototype), GameObject);
                    block.Chunk = this;
                    Blocks.Add(block);
                }
            }
        }
    }

    public override string ToString()
    {
        return string.Format("Chunk ({0}, {1}) #{2}", X, Z, Id);
    }
}

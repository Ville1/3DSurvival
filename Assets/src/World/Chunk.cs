using System.Collections.Generic;
using System.Linq;
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
    public int Average_Elevation { get; private set; }

    private int X_Start { get { return X * SIZE_X; } }
    private int Z_Start { get { return Z * SIZE_Z; } }
    
    private List<List<int>> elevations;

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
        Average_Elevation = 0;
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

    public void Generate_First_Loop(int size_y, int elevation)
    {
        Average_Elevation = elevation;
        elevations = new List<List<int>>();

        for (int x = 0; x < SIZE_X; x++) {
            elevations.Add(new List<int>());
            for (int z = 0; z < SIZE_Z; z++) {
                if(RNG.Instance.Next(0, 100) <= 85) {
                    elevations[x].Add(Average_Elevation);
                } else if(RNG.Instance.Next(0, 100) <= 75) {
                    elevations[x].Add(Average_Elevation + 1);
                } else {
                    elevations[x].Add(Average_Elevation - 1);
                }
            }
        }

        for (int x = X_Start; x < X_Start + SIZE_X; x++) {
            for (int y = 0; y < size_y; y++) {
                for (int z = Z_Start; z < Z_Start + SIZE_Z; z++) {
                    string prototype = BlockPrototypes.AIR_INTERNAL_NAME;
                    int current_elevation = elevations[x - X_Start][z - Z_Start];
                    if (y <= current_elevation - 2) {
                        prototype = "rock";
                    } else if(y == current_elevation - 1) {
                        prototype = "dirt";
                    } else if (y == current_elevation) {
                        prototype = "grass";
                    }
                    Block block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get(prototype), GameObject);
                    block.Chunk = this;
                    Blocks.Add(block);
                }
            }
        }
    }

    public void Generate_Second_Loop()
    {
        //Smoothing
        for (int x = 0; x < SIZE_X; x++) {
            for (int z = 0; z < SIZE_Z; z++) {
                Block grass_block = Blocks.Where(b => b.Coordinates.X == x + X_Start && b.Coordinates.Z == z + Z_Start && b.Internal_Name == "grass").OrderBy(b => b.Coordinates.Y).First();
                List<Block> adjancent_blocks = Map.Get_Adjacent(grass_block, Blocks).Where(b => b.Coordinates.Y == grass_block.Coordinates.Y && b.Internal_Name != BlockPrototypes.AIR_INTERNAL_NAME).ToList();
                if(adjancent_blocks.Count == 0) {
                    grass_block.Change_To(BlockPrototypes.Instance.Air);
                }
            }
        }

        //Slopes
        for (int x = 0; x < SIZE_X; x++) {
            for (int z = 0; z < SIZE_Z; z++) {
                Block air_block = Blocks.Where(b => b.Coordinates.X == x + X_Start && b.Coordinates.Z == z + Z_Start && b.Internal_Name == BlockPrototypes.AIR_INTERNAL_NAME).OrderBy(b => b.Coordinates.Y).FirstOrDefault();
                if(air_block == null) {
                    continue;
                }
                List<Block> adjancent_blocks = Map.Get_Adjacent(air_block, Map.Instance.Get_Blocks_In_Chuck_And_Adjacent_Chunks(this)).Where(b => b.Coordinates.Y == air_block.Coordinates.Y).ToList();
                int count_full_blocks = adjancent_blocks.Where(b => b.Supports_Top).ToList().Count;
                if (count_full_blocks == 1) {
                    air_block.Change_To(BlockPrototypes.Instance.Get("grass_slope"));
                    Block adjacent_block = adjancent_blocks.First(b => b.Supports_Top);
                    if (adjacent_block.Coordinates.X == air_block.Coordinates.X - 1) {
                        air_block.Rotate(0.0f, 90.0f, 0.0f);
                    } else if (adjacent_block.Coordinates.Z == air_block.Coordinates.Z - 1) {
                        //grass_block.Rotate(0.0f, 0.0f, 0.0f);
                    } else if (adjacent_block.Coordinates.X == air_block.Coordinates.X + 1) {
                        air_block.Rotate(0.0f, 270.0f, 0.0f);
                    } else if(adjacent_block.Coordinates.Z == air_block.Coordinates.Z + 1) {
                        air_block.Rotate(0.0f, 180.0f, 0.0f);
                    } else {
                        CustomLogger.Instance.Error("???");
                    }
                }
            }
        }

        //Cover sides
        foreach(Block block in Blocks.Where(x => x.Internal_Name == "dirt")) {
            List<Block> adjancent_blocks = Map.Get_Adjacent(block, Map.Instance.Get_Blocks_In_Chuck_And_Adjacent_Chunks(this));
            if(adjancent_blocks.Exists(x => x.Internal_Name == BlockPrototypes.AIR_INTERNAL_NAME)) {
                block.Change_To(BlockPrototypes.Instance.Get("grass"));
            }
        }
    }

    public void Generate_Third_Loop()
    {
        for (int x = 0; x < SIZE_X; x++) {
            for(int z = 0; z < SIZE_Z; z++) {
                int random = RNG.Instance.Next(0, 100);
                string prototype = null;
                if(random <= 25) {
                    int random2 = RNG.Instance.Next(0, 100);
                    if (random2 < 40) {
                        prototype = "tall_grass";
                    } else if (random2 < 90) {
                        prototype = "medium_grass";
                    } else {
                        prototype = "short_grass";
                    }
                }
                Block air_block = Blocks.Where(b => b.Coordinates.X == x + X_Start && b.Coordinates.Z == z + Z_Start && b.Internal_Name == BlockPrototypes.AIR_INTERNAL_NAME).OrderBy(b => b.Coordinates.Y).First();
                if (prototype != null && air_block != null) {
                    Block block_under = Blocks.Where(b => b.Coordinates.X == x + X_Start && b.Coordinates.Z == z + Z_Start && b.Coordinates.Y == air_block.Coordinates.Y - 1).First();
                    if (block_under != null && block_under.Supports_Top) {
                        air_block.Change_To(BlockPrototypes.Instance.Get(prototype));
                    }
                }
            }
        }
        elevations.Clear();
    }

    public override string ToString()
    {
        return string.Format("Chunk ({0}, {1}) #{2}", X, Z, Id);
    }
}

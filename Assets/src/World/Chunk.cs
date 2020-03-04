using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk {
    public static readonly int SIZE_X = 15;
    public static readonly int SIZE_Z = 15;

    private static long current_id = 0;

    public long Id { get; private set; }
    public int X { get; private set; }
    public int Z { get; private set; }
    public List<Block> Blocks { get; private set; }
    public List<BlockGroup> Block_Groups { get; private set; }
    public GameObject GameObject { get; private set; }
    public int Average_Elevation { get; private set; }
    public bool Simple_Elevation { get; private set; }
    public GenerationTempData Temp_Data { get; private set; }
    public int Size_Y { get; private set; }

    private int X_Start { get { return X * SIZE_X; } }
    private int Z_Start { get { return Z * SIZE_Z; } }

    public Chunk(int x, int z)
    {
        Id = current_id;
        current_id++;
        X = x;
        Z = z;
        Blocks = new List<Block>();
        Block_Groups = new List<BlockGroup>();
        GameObject = new GameObject(string.Format("Chunk_({0},{1})#{2}", X, Z, Id));
        GameObject.transform.parent = Map.Instance.Block_Container.transform;
        GameObject.SetActive(true);
        Average_Elevation = 0;
    }

    public Chunk(ChunkSaveData data)
    {
        Id = data.Id;
        if(current_id <= data.Id) {
            current_id = data.Id + 1;
        }
        X = data.X;
        Z = data.Z;
        Blocks = new List<Block>();
        Block_Groups = new List<BlockGroup>();
        GameObject = new GameObject(string.Format("Chunk_({0},{1})#{2}", X, Z, Id));
        GameObject.transform.parent = Map.Instance.Block_Container.transform;
        GameObject.SetActive(true);
        Average_Elevation = 0;

        foreach(BlockSaveData block_data in data.Blocks) {
            Block block = Block.Load(block_data, GameObject);
            block.Chunk = this;
            Blocks.Add(block);
        }

        foreach(BlockGroupSaveData group_data in data.Block_Groups) {
            BlockGroup group = BlockGroup.Load(group_data, this);
            Block_Groups.Add(group);
        }
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

    public void Generate_First_Loop(int size_y, int elevation, bool simple)
    {
        Size_Y = size_y;
        Average_Elevation = elevation;
        Simple_Elevation = simple;
        Temp_Data = new GenerationTempData();
        Temp_Data.Elevations = new List<List<int>>();
        Temp_Data.Air_Blocks = new List<Block>();
        Temp_Data.Grass_Blocks = new List<Block>();
        Temp_Data.Dirt_Blocks = new List<Block>();
        Temp_Data.Solid_Blocks = new List<Block>();
        Temp_Data.Lowest_Air_Blocks = new List<List<Block>>();
        Temp_Data.Top_Most_Blocks = new List<List<Block>>();
        Temp_Data.All_Blocks = new List<List<List<Block>>>();

        if (Simple_Elevation) {
            Generate_First_Loop_Simple();
        } else {
            Generate_First_Loop_Complex();
        }

        for (int x = X_Start; x < X_Start + SIZE_X; x++) {
            Temp_Data.Lowest_Air_Blocks.Add(new List<Block>());
            Temp_Data.Top_Most_Blocks.Add(new List<Block>());
            Temp_Data.All_Blocks.Add(new List<List<Block>>());
            for (int z = Z_Start; z < Z_Start + SIZE_Z; z++) {
                Temp_Data.All_Blocks[x - X_Start].Add(new List<Block>());
                Block top_most = null;
                Block lowest_air = null;
                for (int y = 0; y < Size_Y; y++) {
                    int current_elevation = Temp_Data.Elevations[x - X_Start][z - Z_Start];
                    Block block = null;
                    if (y == 0) {
                        block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get("bed_rock"), GameObject);
                    } else if (y <= current_elevation - 2) {
                        block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get("rock"), GameObject);
                    } else if (y == current_elevation - 1) {
                        block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get("dirt"), GameObject);
                        Temp_Data.Dirt_Blocks.Add(block);
                    } else if (y == current_elevation) {
                        block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Get("grass"), GameObject);
                        Temp_Data.Grass_Blocks.Add(block);
                        top_most = block;
                    } else {
                        block = new Block(new Coordinates(x, y, z), BlockPrototypes.Instance.Air, GameObject);
                        Temp_Data.Air_Blocks.Add(block);
                        if (y == current_elevation + 1) {
                            lowest_air = block;
                        }
                    }
                    if (!block.Is_Air) {
                        Temp_Data.Solid_Blocks.Add(block);
                    }
                    block.Chunk = this;
                    Blocks.Add(block);
                    Temp_Data.All_Blocks[x - X_Start][z - Z_Start].Add(block);
                }
                Temp_Data.Top_Most_Blocks[x - X_Start].Add(top_most);
                Temp_Data.Lowest_Air_Blocks[x - X_Start].Add(lowest_air);
            }
        }
    }

    private void Generate_First_Loop_Simple()
    {
        for (int x = 0; x < SIZE_X; x++) {
            Temp_Data.Elevations.Add(new List<int>());
            for (int z = 0; z < SIZE_Z; z++) {
                Temp_Data.Elevations[x].Add(Average_Elevation);
            }
        }
    }

    private void Generate_First_Loop_Complex()
    {
        for (int x = 0; x < SIZE_X; x++) {
            Temp_Data.Elevations.Add(new List<int>());
            for (int z = 0; z < SIZE_Z; z++) {
                if (RNG.Instance.Next(0, 100) <= 85) {
                    Temp_Data.Elevations[x].Add(Average_Elevation);
                } else if (RNG.Instance.Next(0, 100) <= 85) {
                    Temp_Data.Elevations[x].Add(Average_Elevation + 1);
                } else {
                    Temp_Data.Elevations[x].Add(Average_Elevation - 1);
                }
            }
        }
    }

    public void Generate_Second_Loop()
    {
        if (Simple_Elevation) {
            Generate_Second_Loop_Simple();
        } else {
            Generate_Second_Loop_Complex();
        }
    }

    private void Generate_Second_Loop_Simple()
    {
        //Hills
        List<Block> new_dirt = new List<Block>();
        int hill_count = Mathf.RoundToInt(((SIZE_X * SIZE_Z) / 75.0f) * RNG.Instance.Next_F());
        for (int i = 0; i < hill_count; i++) {
            int hill_start_x = Mathf.RoundToInt((SIZE_X / 2.0f) * RNG.Instance.Next_F());
            int hill_start_z = Mathf.RoundToInt((SIZE_Z / 2.0f) * RNG.Instance.Next_F());
            int hill_size_x = Mathf.RoundToInt((SIZE_X / 2.0f) * RNG.Instance.Next_F());
            int hill_size_z = Mathf.RoundToInt((SIZE_Z / 2.0f) * RNG.Instance.Next_F());
            
            for (int x = hill_start_x; x < hill_start_x + hill_size_x; x++) {
                for (int z = hill_start_z; z < hill_start_z + hill_size_z; z++) {
                    Block block_under = Temp_Data.Top_Most_Blocks[x][z];
                    Block block = Temp_Data.Lowest_Air_Blocks[x][z];
                    if (block == null) {
                        continue;
                    }
                    block.Change_To(BlockPrototypes.Instance.Get("grass"));
                    block_under.Change_To(BlockPrototypes.Instance.Get("dirt"));
                    new_dirt.Add(block_under);
                    Temp_Data.Air_Blocks.Remove(block);
                    Temp_Data.Grass_Blocks.Add(block);
                    Temp_Data.Dirt_Blocks.Add(block_under);
                    Temp_Data.Grass_Blocks.Remove(block_under);

                    Temp_Data.Top_Most_Blocks[x][z] = block;
                    if (block.Coordinates.Y + 1 < Size_Y) {
                        Temp_Data.Lowest_Air_Blocks[x][z] = Temp_Data.All_Blocks[x][z][block.Coordinates.Y + 1];
                    } else {
                        Temp_Data.Lowest_Air_Blocks[x][z] = null;
                    }
                }
            }

            int side_min_x = hill_start_x - 1;
            int side_min_z = hill_start_z - 1;
            int side_max_x = hill_start_x + hill_size_x;
            int side_max_z = hill_start_z + hill_size_z;
            if(side_min_x >= 0) {
                for(int z = side_min_z; z <= side_max_z; z++) {
                    if(z < 0 || z >= SIZE_Z) {
                        continue;
                    }
                    Block block = Temp_Data.Air_Blocks.FirstOrDefault(x => x.Coordinates.X == X_Start + side_min_x && x.Coordinates.Z == Z_Start + z && x.Coordinates.Y == Temp_Data.Top_Most_Blocks[hill_start_x][z].Coordinates.Y);
                    if(block == null) {
                        continue;
                    }
                    if (block.Coordinates.Y > 0 && !Temp_Data.All_Blocks[side_min_x][z][block.Coordinates.Y - 1].Supports_Top) {
                        continue;
                    }
                    block.Change_To(BlockPrototypes.Instance.Get("grass_slope"));
                    Temp_Data.Air_Blocks.Remove(block);
                    block.Rotate(0.0f, 270.0f, 0.0f);
                }
            }

            if (side_min_z >= 0) {
                for (int x = side_min_x; x <= side_max_x; x++) {
                    if (x < 0 || x >= SIZE_X) {
                        continue;
                    }
                    Block block = Temp_Data.Air_Blocks.FirstOrDefault(b => b.Coordinates.X == X_Start + x && b.Coordinates.Z == Z_Start + side_min_z && b.Coordinates.Y == Temp_Data.Top_Most_Blocks[x][hill_start_z].Coordinates.Y);
                    if (block == null) {
                        continue;
                    }
                    if (block.Coordinates.Y > 0 && !Temp_Data.All_Blocks[x][side_min_z][block.Coordinates.Y - 1].Supports_Top) {
                        continue;
                    }
                    block.Change_To(BlockPrototypes.Instance.Get("grass_slope"));
                    Temp_Data.Air_Blocks.Remove(block);
                    block.Rotate(0.0f, 180.0f, 0.0f);
                }
            }

            if (side_max_x < SIZE_X) {
                for (int z = side_min_z; z <= side_max_z; z++) {
                    if (z < 0 || z >= SIZE_Z) {
                        continue;
                    }
                    Block block = Temp_Data.Air_Blocks.FirstOrDefault(x => x.Coordinates.X == X_Start + side_max_x && x.Coordinates.Z == Z_Start + z && x.Coordinates.Y == Temp_Data.Top_Most_Blocks[hill_start_x + hill_size_x - 1][z].Coordinates.Y);
                    if (block == null) {
                        continue;
                    }
                    if (block.Coordinates.Y > 0 && !Temp_Data.All_Blocks[side_max_x][z][block.Coordinates.Y - 1].Supports_Top) {
                        continue;
                    }
                    block.Change_To(BlockPrototypes.Instance.Get("grass_slope"));
                    Temp_Data.Air_Blocks.Remove(block);
                    block.Rotate(0.0f, 90.0f, 0.0f);
                }
            }

            if (side_max_z < SIZE_Z) {
                for (int x = side_min_x; x <= side_max_x; x++) {
                    if (x < 0 || x >= SIZE_X) {
                        continue;
                    }
                    //TODO
                    //ArgumentOutOfRangeException: Argument is out of range.
                    //Parameter name: index
                    Block block = Temp_Data.Air_Blocks.FirstOrDefault(b => b.Coordinates.X == X_Start + x && b.Coordinates.Z == Z_Start + side_max_z && b.Coordinates.Y == Temp_Data.Top_Most_Blocks[x][hill_start_z + hill_size_z - 1].Coordinates.Y);
                    if (block == null) {
                        continue;
                    }
                    if (block.Coordinates.Y > 0 && !Temp_Data.All_Blocks[x][side_max_z][block.Coordinates.Y - 1].Supports_Top) {
                        continue;
                    }
                    block.Change_To(BlockPrototypes.Instance.Get("grass_slope"));
                    Temp_Data.Air_Blocks.Remove(block);
                    //block.Rotate(0.0f, 0.0f, 0.0f);
                }
            }
        }

        //Cover dirt sides
        foreach (Block block in new_dirt) {
            List<Block> possible_adjacent_air = Helper.Clone_List(Temp_Data.Air_Blocks);
            foreach (Chunk chunck in Map.Instance.Get_Adjacent_Chunks(this)) {
                if (chunck.Temp_Data != null) {
                    foreach (Block b in chunck.Temp_Data.Air_Blocks) {
                        possible_adjacent_air.Add(b);
                    }
                } else {
                    foreach (Block b in chunck.Blocks) {
                        if (b.Is_Air) {
                            possible_adjacent_air.Add(b);
                        }
                    }
                }
            }
            List<Block> adjancent_blocks_air = Map.Get_Adjacent(block, possible_adjacent_air);
            if (adjancent_blocks_air.Count != 0) {
                block.Change_To(BlockPrototypes.Instance.Get("grass"));
            }
        }
    }

    private void Generate_Second_Loop_Complex()
    {
        //Smoothing
        for (int x = 0; x < SIZE_X; x++) {
            for (int z = 0; z < SIZE_Z; z++) {
                Block grass_block = Temp_Data.Top_Most_Blocks[x][z];
                List<Block> adjancent_air_blocks = new List<Block>();
                if(x > 0) {
                    Block b = Temp_Data.All_Blocks[x - 1][z][grass_block.Coordinates.Y];
                    if(b.Is_Air) {
                        adjancent_air_blocks.Add(b);
                    }
                }
                if (z > 0) {
                    Block b = Temp_Data.All_Blocks[x][z - 1][grass_block.Coordinates.Y];
                    if (b.Is_Air) {
                        adjancent_air_blocks.Add(b);
                    }
                }
                if (x < SIZE_X - 1) {
                    Block b = Temp_Data.All_Blocks[x + 1][z][grass_block.Coordinates.Y];
                    if (b.Is_Air) {
                        adjancent_air_blocks.Add(b);
                    }
                }
                if (z < SIZE_Z - 1) {
                    Block b = Temp_Data.All_Blocks[x][z + 1][grass_block.Coordinates.Y];
                    if (b.Is_Air) {
                        adjancent_air_blocks.Add(b);
                    }
                }
                if (adjancent_air_blocks.Count == 4) {
                    grass_block.Change_To(BlockPrototypes.Instance.Air);
                    Temp_Data.Top_Most_Blocks[x][z] = Temp_Data.All_Blocks[x][z][grass_block.Coordinates.Y - 1];
                    Temp_Data.Lowest_Air_Blocks[x][z] = grass_block;
                    Temp_Data.Grass_Blocks.Remove(grass_block);
                    Temp_Data.Solid_Blocks.Remove(grass_block);

                    Block top = Temp_Data.Top_Most_Blocks[x][z];
                    if (top.Internal_Name == "dirt") {
                        top.Change_To(BlockPrototypes.Instance.Get("grass"));
                        Temp_Data.Grass_Blocks.Add(top);
                    }
                }
            }
        }

        //Slopes
        for (int x = 0; x < SIZE_X; x++) {
            for (int z = 0; z < SIZE_Z; z++) {
                Block air_block = Temp_Data.Lowest_Air_Blocks[x][z];
                List<Block> possible_connections = Helper.Clone_List(Temp_Data.Solid_Blocks);
                foreach (Chunk chunck in Map.Instance.Get_Adjacent_Chunks(this)) {
                    if (chunck.Temp_Data != null) {
                        foreach (Block b in chunck.Temp_Data.Solid_Blocks) {
                            possible_connections.Add(b);
                        }
                    } else {
                        foreach (Block b in chunck.Blocks) {
                            if (b.Supports_Top) {
                                possible_connections.Add(b);
                            }
                        }
                    }
                }
                List<Block> adjancent_blocks = Map.Get_Adjacent(air_block, possible_connections).Where(b => b.Coordinates.Y == air_block.Coordinates.Y).ToList();
                int count_full_blocks = adjancent_blocks.Count;
                if (count_full_blocks == 1) {
                    air_block.Change_To(BlockPrototypes.Instance.Get("grass_slope"));
                    Block adjacent_block = adjancent_blocks.First(b => b.Supports_Top);
                    if (adjacent_block.Coordinates.X == air_block.Coordinates.X - 1) {
                        air_block.Rotate(0.0f, 90.0f, 0.0f);
                    } else if (adjacent_block.Coordinates.Z == air_block.Coordinates.Z - 1) {
                        //grass_block.Rotate(0.0f, 0.0f, 0.0f);
                    } else if (adjacent_block.Coordinates.X == air_block.Coordinates.X + 1) {
                        air_block.Rotate(0.0f, 270.0f, 0.0f);
                    } else if (adjacent_block.Coordinates.Z == air_block.Coordinates.Z + 1) {
                        air_block.Rotate(0.0f, 180.0f, 0.0f);
                    } else {
                        CustomLogger.Instance.Error("???");
                    }
                }
            }
        }
        
        //Cover sides
        foreach (Block block in Temp_Data.Dirt_Blocks) {
            List<Block> possible_adjacent_air = Helper.Clone_List(Temp_Data.Air_Blocks);
            foreach (Chunk chunck in Map.Instance.Get_Adjacent_Chunks(this)) {
                if (chunck.Temp_Data != null) {
                    foreach (Block b in chunck.Temp_Data.Air_Blocks) {
                        possible_adjacent_air.Add(b);
                    }
                } else {
                    foreach (Block b in chunck.Blocks) {
                        if (b.Is_Air) {
                            possible_adjacent_air.Add(b);
                        }
                    }
                }
            }
            List<Block> adjancent_blocks_air = Map.Get_Adjacent(block, possible_adjacent_air);
            if(adjancent_blocks_air.Count != 0) {
                block.Change_To(BlockPrototypes.Instance.Get("grass"));
            }
        }
    }

    public void Generate_Third_Loop()
    {
        for (int x = 0; x < SIZE_X; x++) {
            for(int z = 0; z < SIZE_Z; z++) {
                //Details
                int random = RNG.Instance.Next(0, 100);
                string prototype = null;
                if(random <= 10) {
                    int random2 = RNG.Instance.Next(0, 100);
                    if (random2 < 40) {
                        prototype = "tall_grass";
                    } else if (random2 < 90) {
                        prototype = "medium_grass";
                    } else {
                        prototype = "short_grass";
                    }
                } else if(random <= 15) {
                    prototype = "stones";
                } else if (random <= 20) {
                    prototype = "sticks";
                } else if (random <= 23) {
                    prototype = "flint";
                } else if (random <= 30) {
                    prototype = "trunk";
                }
                Block air_block = Temp_Data.Air_Blocks.Where(b => b.Coordinates.X == x + X_Start && b.Coordinates.Z == z + Z_Start).OrderBy(b => b.Coordinates.Y).FirstOrDefault();
                if (prototype != null && air_block != null) {
                    Block block_under = Blocks.Where(b => b.Coordinates.X == x + X_Start && b.Coordinates.Z == z + Z_Start && b.Coordinates.Y == air_block.Coordinates.Y - 1).First();
                    if (block_under != null && block_under.Supports_Top) {
                        air_block.Change_To(BlockPrototypes.Instance.Get(prototype));
                        if(prototype == "trunk") {
                            Generate_Tree(air_block);
                        }
                    }
                }

                //Support
                Block bottom = null;
                for(int y = 0; y < Size_Y; y++) {
                    if(bottom == null) {
                        Temp_Data.All_Blocks[x][z][y].Base_Pilar_Support = true;
                    } else {
                        if (bottom.Connections.Is_Connected_To(ConnectionData.Direction.Top, Temp_Data.All_Blocks[x][z][y].Connections)) {
                            Temp_Data.All_Blocks[x][z][y].Base_Pilar_Support = true;
                        } else {
                            break;
                        }
                    }
                    bottom = Temp_Data.All_Blocks[x][z][y];
                }
            }
        }
    }

    private void Generate_Tree(Block start)
    {
        BlockGroup tree = new BlockGroup("Tree", BlockGroup.GroupType.Tree);
        tree.Add(start);
        int max_height = Size_Y - start.Coordinates.Y;
        if(max_height <= 1) {
            return;
        }
        int height = max_height;
        if(max_height > 2) {
            height = Mathf.RoundToInt(2 + (max_height - 2) * RNG.Instance.Next_F());
        }
        if(height == 2) {
            Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start][start.Coordinates.Y + 1].Change_To(BlockPrototypes.Instance.Get("leaves"));
            return;
        }
        for(int h = start.Coordinates.Y + 1; h < start.Coordinates.Y + height; h++) {
            if (h < start.Coordinates.Y + height - 1) {
                Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start][h].Change_To(BlockPrototypes.Instance.Get("trunk"));
            } else {
                Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start][h].Change_To(BlockPrototypes.Instance.Get("leaves"));
            }
            tree.Add(Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start][h]);
        }
        int top = start.Coordinates.Y + height - 1;
        if (start.Coordinates.X - X_Start - 1 >= 0 && Temp_Data.All_Blocks[start.Coordinates.X - X_Start - 1][start.Coordinates.Z - Z_Start][top].Is_Air) {
            Temp_Data.All_Blocks[start.Coordinates.X - X_Start - 1][start.Coordinates.Z - Z_Start][top].Change_To(BlockPrototypes.Instance.Get("leaves"));
            tree.Add(Temp_Data.All_Blocks[start.Coordinates.X - X_Start - 1][start.Coordinates.Z - Z_Start][top]);
        }
        if (start.Coordinates.Z - Z_Start - 1 >= 0 && Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start - 1][top].Is_Air) {
            Temp_Data.All_Blocks[start.Coordinates.X - X_Start ][start.Coordinates.Z - Z_Start - 1][top].Change_To(BlockPrototypes.Instance.Get("leaves"));
            tree.Add(Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start - 1][top]);
        }
        if (start.Coordinates.X - X_Start + 1 < SIZE_X && Temp_Data.All_Blocks[start.Coordinates.X - X_Start + 1][start.Coordinates.Z - Z_Start][top].Is_Air) {
            Temp_Data.All_Blocks[start.Coordinates.X - X_Start + 1][start.Coordinates.Z - Z_Start][top].Change_To(BlockPrototypes.Instance.Get("leaves"));
            tree.Add(Temp_Data.All_Blocks[start.Coordinates.X - X_Start + 1][start.Coordinates.Z - Z_Start][top]);
        }
        if (start.Coordinates.Z - Z_Start + 1 < SIZE_Z && Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start + 1][top].Is_Air) {
            Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start + 1][top].Change_To(BlockPrototypes.Instance.Get("leaves"));
            tree.Add(Temp_Data.All_Blocks[start.Coordinates.X - X_Start][start.Coordinates.Z - Z_Start + 1][top]);
        }
        //TODO: grass end up in here? Because index errors?
        tree.Blocks = tree.Blocks.Where(x => x.Internal_Name == "trunk" || x.Internal_Name == "leaves").ToList();
        Block_Groups.Add(tree);
    }

    public void End_Generation()
    {
        Temp_Data = null;
    }

    public void Save()
    {
        ChunkSaveData data = new ChunkSaveData();
        data.Id = Id;
        data.X = X;
        data.Z = Z;
        data.Blocks = new List<BlockSaveData>();
        foreach(Block block in Blocks) {
            data.Blocks.Add(block.Save_Data);
        }
        data.Block_Groups = new List<BlockGroupSaveData>();
        foreach (BlockGroup group in Block_Groups) {
            data.Block_Groups.Add(group.Save_Data);
        }

        SaveManager.Instance.Add(data);
    }

    public static void Reset_Current_Id()
    {
        current_id = 0;
    }

    public override string ToString()
    {
        return string.Format("Chunk ({0}, {1}) #{2}", X, Z, Id);
    }

    public class GenerationTempData
    {
        public List<List<int>> Elevations { get; set; }
        public List<Block> Air_Blocks { get; set; }
        public List<Block> Grass_Blocks { get; set; }
        public List<Block> Dirt_Blocks { get; set; }
        public List<Block> Solid_Blocks { get; set; }
        public List<List<Block>> Lowest_Air_Blocks { get; set; }
        public List<List<Block>> Top_Most_Blocks { get; set; }
        public List<List<List<Block>>> All_Blocks { get; set; }
    }
}

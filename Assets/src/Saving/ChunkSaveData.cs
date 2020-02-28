using System;
using System.Collections.Generic;

[Serializable]
public class ChunkSaveData {
    public long Id;
    public int X;
    public int Z;

    public List<BlockSaveData> Blocks;
}

using System;
using System.Collections.Generic;

[Serializable]
public class BlockGroupSaveData {
    public long Id;
    public string Name;
    public int Type;
    public List<long> Blocks;
}

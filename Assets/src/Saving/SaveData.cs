using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
    public PlayerSaveData Player;
    public List<ChunkSaveData> Chunks;
}

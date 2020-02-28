using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
    public CoordinatesSaveData Player_Coordinates;
    public CoordinatesSaveData Player_Spawn;
    public List<ChunkSaveData> Chunks;
}

using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
    public PlayerSaveData Player;
    public List<ChunkSaveData> Chunks;
    public bool Structural_Integrity_Enabled;
}

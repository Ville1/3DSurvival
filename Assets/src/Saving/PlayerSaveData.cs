using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData {
    public CoordinatesSaveData Coordinates;
    public CoordinatesSaveData Spawn;
    public List<ItemSaveData> Items;
}

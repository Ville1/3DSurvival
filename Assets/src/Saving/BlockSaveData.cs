using System;

[Serializable]
public class BlockSaveData {
    public long Id;
    public CoordinatesSaveData Coordinates;
    public string Internal_Name;
    public CoordinatesSaveData Rotation;
    public bool Base_Pilar_Support;
}

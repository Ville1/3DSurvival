using System;

public class Direction {
    public enum Shift { Positive = 1, None = 0, Negative = -1 }

    public Shift X { get; set; }
    public Shift Y { get; set; }
    public Shift Z { get; set; }

    public Direction()
    {
        X = Shift.None;
        Y = Shift.None;
        Z = Shift.None;
    }

    public Direction(Shift x, Shift y, Shift z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Direction(int x, int y, int z)
    {
        try {
            X = (Shift)x;
            Y = (Shift)y;
            Z = (Shift)z;
        } catch (Exception) {
            CustomLogger.Instance.Error("Parsing error");
            X = Shift.None;
            Y = Shift.None;
            Z = Shift.None;
        }
    }

    public bool Is_Empty
    {
        get {
            return X == Shift.None && Y == Shift.None && Z == Shift.None;
        }
    }
    
    /// <summary>
    /// TODO: proper math
    /// </summary>
    public float X_Multiplier
    {
        get {
            return X == Shift.None ? 0.0f : (Y != Shift.None || Z != Shift.None ? 0.70f * (int)X : (int)X);
        }
    }

    public float Y_Multiplier
    {
        get {
            return Y == Shift.None ? 0.0f : (X != Shift.None || Z != Shift.None ? 0.70f * (int)Y : (int)Y);
        }
    }

    public float Z_Multiplier
    {
        get {
            return Z == Shift.None ? 0.0f : (X != Shift.None || Y != Shift.None ? 0.70f * (int)Z : (int)Z);
        }
    }

    public override string ToString()
    {
        return string.Format("Direction X: {0}, Y: {1}, Z: {2}", X, Y, Z);
    }
}

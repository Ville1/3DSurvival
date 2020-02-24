using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Coordinates
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public Coordinates(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Coordinates(Coordinates coordinates)
    {
        X = coordinates.X;
        Y = coordinates.Y;
        Z = coordinates.Z;
    }

    public Coordinates(Vector3 vector)
    {
        X = (int)vector.x;
        Y = (int)vector.y;
        Z = (int)vector.z;
    }

    public Vector3 Vector
    {
        get {
            return new Vector3(X, Y, Z);
        }
        set {
            X = (int)value.x;
            Y = (int)value.y;
            Z = (int)value.z;
        }
    }
    
    public Coordinates Shift(Coordinates coordinates)
    {
        X += coordinates.X;
        Y += coordinates.Y;
        Z += coordinates.Z;
        return this;
    }
    
    public string Parse_Text(bool brackets, bool spaces)
    {
        StringBuilder builder = new StringBuilder();
        if (brackets) {
            builder.Append("(");
        }
        builder.Append("X:");
        if (spaces) {
            builder.Append(" ");
        }
        builder.Append(X).Append(",");
        if (spaces) {
            builder.Append(" ");
        }
        builder.Append("Y:");
        if (spaces) {
            builder.Append(" ");
        }
        builder.Append(Y).Append(",");
        if (spaces) {
            builder.Append(" ");
        }
        builder.Append("Z:");
        if (spaces) {
            builder.Append(" ");
        }
        builder.Append(Z);
        if (brackets) {
            builder.Append(")");
        }
        return builder.ToString();
    }

    public override string ToString()
    {
        return string.Format("Coordinates{0}", Parse_Text(true, true));
    }

    public override bool Equals(object obj)
    {
        if (obj is Coordinates) {
            return ((Coordinates)obj).X == X && ((Coordinates)obj).Y == Y && ((Coordinates)obj).Z == Z;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return int.Parse(string.Format("{0}{1}{2}", X, Y, Z));
    }

    public float Distance(Coordinates coordinates)
    {
        return Mathf.Sqrt((X - coordinates.X) * (X - coordinates.X) + (Y - coordinates.Y) * (Y - coordinates.Y) + (Z - coordinates.Z) * (Z - coordinates.Z));
    }
}

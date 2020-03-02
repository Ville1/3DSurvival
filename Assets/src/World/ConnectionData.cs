using System;
using System.Collections.Generic;

public class ConnectionData {
    public enum Direction { Top, Bottom, Positive_X, Positive_Z, Negative_X, Negative_Z }

    private List<Direction> connections;

    public bool Top { get { return connections != null ? connections.Contains(Direction.Top) : false; } }
    public bool Bottom { get { return connections != null ? connections.Contains(Direction.Bottom) : false; } }
    public bool Positive_X { get { return connections != null ? connections.Contains(Direction.Positive_X) : false; } }
    public bool Positive_Z { get { return connections != null ? connections.Contains(Direction.Positive_Z) : false; } }
    public bool Negative_X { get { return connections != null ? connections.Contains(Direction.Negative_X) : false; } }
    public bool Negative_Z { get { return connections != null ? connections.Contains(Direction.Negative_Z) : false; } }
    public bool Empty { get { return connections != null ? connections.Count != 0 : false; } }

    public ConnectionData()
    {
        connections = new List<Direction>();
    }

    public ConnectionData(bool all)
    {
        connections = new List<Direction>();
        if (all) {
            foreach(Direction d in Enum.GetValues(typeof(Direction))) {
                connections.Add(d);
            }
        }
    }

    public ConnectionData(Direction direction)
    {
        connections = new List<Direction>() { direction };
    }

    public ConnectionData(List<Direction> directions)
    {
        connections = Helper.Clone_List(directions);
    }

    public ConnectionData(ConnectionData data)
    {
        connections = Helper.Clone_List(data.connections);
    }

    public List<Direction> Lost_Connetions(ConnectionData last_data)
    {
        List<Direction> lost = new List<Direction>();
        foreach(Direction d in last_data.connections) {
            if (!connections.Contains(d)) {
                lost.Add(d);
            }
        }
        return lost;
    }

    public bool Is_Connected_To(Direction direction, ConnectionData data)
    {
        switch (direction) {
            case Direction.Bottom:
                return Bottom && data.Top;
            case Direction.Top:
                return Top && data.Bottom;
            case Direction.Negative_X:
                return Negative_X && data.Positive_X;
            case Direction.Positive_X:
                return Positive_X && data.Negative_X;
            case Direction.Negative_Z:
                return Negative_Z && data.Positive_Z;
            case Direction.Positive_Z:
                return Positive_Z && data.Negative_Z;
        }
        //This should not happen
        CustomLogger.Instance.Error("???");
        return false;
    }
    
    public List<Direction> All
    {
        get {
            return Helper.Clone_List(connections);
        }
    }

    public static Coordinates To_Coordinate_Delta(Direction direction)
    {
        switch (direction) {
            case Direction.Bottom:
                return new Coordinates(0, -1, 0);
            case Direction.Top:
                return new Coordinates(0, 1, 0);
            case Direction.Negative_X:
                return new Coordinates(-1, 0, 0);
            case Direction.Positive_X:
                return new Coordinates(1, 0, 0);
            case Direction.Negative_Z:
                return new Coordinates(0, 0, -1);
            case Direction.Positive_Z:
                return new Coordinates(0, 0, 1);
        }
        return new Coordinates(0, 0, 0);
    }
}

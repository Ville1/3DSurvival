using UnityEngine;

public class Player : Mob
{
    public Player(Vector3 position, Player prototype, GameObject container) : base(position, prototype, container)
    {

    }

    public Player(string name, string prefab_name) : base(name, prefab_name, null, MaterialManager.MaterialType.Block, null)
    {

    }

    public static Player Current
    {
        get {
            return Map.Instance.Players == null || Map.Instance.Players.Count == 0 ? null : Map.Instance.Players[0];
        }
    }
}

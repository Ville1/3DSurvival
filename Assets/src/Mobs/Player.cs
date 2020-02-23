using UnityEngine;

public class Player : Mob
{
    public static readonly string PLAYER_GAME_OBJECT_NAME_PREFIX = "Player_";

    private static long current_id;

    public long Id { get; private set; }

    public Player(Vector3 position, Player prototype, GameObject container) : base(position, prototype, container)
    {
        Id = current_id;
        current_id++;
        GameObject.name = string.Format("{0}#{1}", PLAYER_GAME_OBJECT_NAME_PREFIX, Id);
    }

    public Player(string name, string prefab_name, float movement_speed, float jump_strenght) : base(name, prefab_name, null, MaterialManager.MaterialType.Block, null, movement_speed, jump_strenght)
    {
        Id = -1;
    }

    public static Player Current
    {
        get {
            return Map.Instance.Players == null || Map.Instance.Players.Count == 0 ? null : Map.Instance.Players[0];
        }
    }

    public Camera Camera
    {
        get {
            return GameObject != null ? GameObject.GetComponentInChildren<Camera>() : null;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Player : Mob
{
    public static readonly string PLAYER_GAME_OBJECT_NAME_PREFIX = "Player_";

    private static long current_id;

    public long Player_Id { get; private set; }

    public Player(Vector3 position, Player prototype, GameObject container) : base(position, prototype, container)
    {
        Player_Id = current_id;
        current_id++;
        GameObject.name = string.Format("{0}#{1}", PLAYER_GAME_OBJECT_NAME_PREFIX, Player_Id);
        Inventory = prototype.Inventory.Clone();
    }

    public Player(string name, string prefab_name, float movement_speed, float jump_strenght, int hp, List<Skill> skills, Dictionary<string, int> starting_items, float dismantling_speed, float building_speed,
        float crafting_speed, float max_weight, float max_volyme) :
        base(name, prefab_name, null, null, null, movement_speed, jump_strenght, hp, skills, dismantling_speed, building_speed, crafting_speed, true)
    {
        Player_Id = -1;
        Inventory = new Inventory(starting_items, max_weight, max_volyme);
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

    public static Player Prototype
    {
        get {
            return new Player("Player 1", "Player", 1.0f, 1.0f, 50, new List<Skill>() { new Skill("Mining", Skill.SkillId.Mining, 3), new Skill("Masonry", Skill.SkillId.Masonry, 1) }, new Dictionary<string, int>() {
                { "dev_hammer", 1 },
                { "dev_pickaxe", 1 },
                { "dev_shovel", 1 }
            }, 1.0f, 1.0f, 1.0f, 100.0f, 95.0f);
        }
    }
}

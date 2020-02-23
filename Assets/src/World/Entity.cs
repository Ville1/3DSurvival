using UnityEngine;

public class Entity : MapObject {
    public static readonly string GAME_OBJECT_NAME_PREFIX = "Entity_";

    private static long current_id;
    
    public long Id { get; private set; }

    public Entity(Vector3 position, Entity prototype, GameObject container) : base(prototype.Name, position, container, prototype.Prefab_Name, prototype.Material, prototype.Material_Type,
        prototype.Model_Name, true)
    {
        Id = current_id;
        current_id++;

        GameObject.name = string.Format("{0}#{1}", GAME_OBJECT_NAME_PREFIX, Id);
        Map.Instance.Add_Entity(this);
    }

    public Entity(string name, string prefab_name, string material, MaterialManager.MaterialType material_type, string model_name) : base(name, prefab_name, material, material_type, model_name)
    {
        Id = -1;
    }

    public void Update(float delta_time)
    {

    }

    public new void Delete()
    {
        base.Delete();
        Map.Instance.Remove_Entity(this);
    }
}

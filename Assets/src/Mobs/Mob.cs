using UnityEngine;

public class Mob : Entity {
    public Mob(Vector3 position, Mob prototype, GameObject container) : base(position, prototype, container)
    {

    }

    public Mob(string name, string prefab_name, string material, MaterialManager.MaterialType material_type, string model_name) : base(name, prefab_name, material, material_type, model_name)
    {

    }

    public new void Update(float delta_time)
    {

    }
}

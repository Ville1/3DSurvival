using UnityEngine;

public class Entity : MapObject {
    public static readonly string GAME_OBJECT_NAME_PREFIX = "Entity_";

    private static long current_id;
    
    public long Id { get; private set; }
    public Inventory Inventory { get; protected set; }

    private bool paused;
    private bool is_kinematic;

    public Entity(Vector3 position, Entity prototype, GameObject container) : base(prototype.Name, position, container, new PrototypeData(prototype.Prefab_Name, prototype.Material, prototype.Material_Type), true)
    {
        Id = current_id;
        current_id++;
        Inventory = new Inventory();

        GameObject.name = string.Format("{0}#{1}", GAME_OBJECT_NAME_PREFIX, Id);
        Map.Instance.Add_Entity(this);
        paused = false;
    }

    public Entity(string name, string prefab_name, string material, MaterialManager.MaterialType? material_type, string model_name) : base(name, prefab_name, material, material_type, model_name)
    {
        Id = -1;
        Inventory = new Inventory();
        paused = false;
    }

    public void Update(float delta_time)
    {

    }

    public new void Delete()
    {
        base.Delete();
        Map.Instance.Remove_Entity(this);
    }

    public CollisionBehaviour Collision_Data
    {
        get {
            return GameObject == null ? null : GameObject.GetComponentInChildren<CollisionBehaviour>();
        }
    }

    protected Rigidbody Rigidbody
    {
        get {
            return GameObject == null ? null : GameObject.GetComponentInChildren<Rigidbody>();
        }
    }

    public static long? Parse_Id_From_GameObject_Name(string name)
    {
        if (!name.Contains(GAME_OBJECT_NAME_PREFIX) || !name.Contains("#")) {
            return null;
        }
        long id = 0;
        if (!long.TryParse(name.Substring(name.IndexOf("#") + 1), out id)) {
            CustomLogger.Instance.Error(string.Format("Error parsing string: {0}", name));
            return null;
        }
        return id;
    }

    public bool Paused
    {
        get {
            return paused;
        }
        set {
            if(paused == value) {
                return;
            }
            paused = value;
            if (!is_kinematic && Rigidbody != null) {
                Rigidbody.isKinematic = paused;
            }
        }
    }
}

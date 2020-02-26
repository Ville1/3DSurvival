using UnityEngine;

public class MapObject {
    public string Name { get; protected set; }
    public GameObject GameObject { get; protected set; }
    public GameObject Parent { get; protected set; }
    public string Prefab_Name { get; protected set; }
    public string Material { get; protected set; }
    public MaterialManager.MaterialType? Material_Type { get; protected set; }
    public string Model_Name { get; protected set; }

    public MapObject(string name, Vector3 position, GameObject parent, PrototypeData prototype, bool active)
    {
        Name = name;
        Parent = parent;
        Prefab_Name = prototype.Use_Prefab ? prototype.Prefab_Name : null;
        Material = prototype.Use_Prefab ? prototype.Material : null;
        Material_Type = prototype.Use_Prefab ? prototype.Material_Type : null;
        Model_Name = prototype.Use_Prefab ? null : prototype.Model_Name;

        GameObject = GameObject.Instantiate(
            prototype.Use_Prefab ? PrefabManager.Instance.Get(prototype.Prefab_Name) : ModelManager.Instance.Get(prototype.Model_Name),
            position,
            Quaternion.identity,
            Parent.transform
        );
        GameObject.name = Name;
        if (!prototype.Use_Prefab) {
            BoxCollider collider = GameObject.AddComponent<BoxCollider>();
            collider.isTrigger = !prototype.Has_Collision;
        }

        GameObject.SetActive(active);
        Update_Material();
    }
    
    public MapObject(string name, string prefab_name, string material, MaterialManager.MaterialType? material_type, string model_name)
    {
        Name = name;
        Prefab_Name = prefab_name;
        Material = material;
        Material_Type = material_type;
        Model_Name = model_name;
    }

    public Vector3 Position
    {
        get {
            return GameObject == null ? new Vector3() : new Vector3(GameObject.transform.position.x, GameObject.transform.position.y, GameObject.transform.position.z);
        }
        set {
            if(GameObject == null) {
                return;
            }
            GameObject.transform.position = new Vector3(
                value.x,
                value.y,
                value.z
            );
        }
    }

    public bool Active
    {
        get {
            return GameObject == null ? false : GameObject.activeSelf;
        }
        set {
            if(GameObject == null) {
                return;
            }
            GameObject.SetActive(value);
        }
    }

    public MeshRenderer MeshRenderer
    {
        get {
            return GameObject != null ? GameObject.GetComponentInChildren<MeshRenderer>() : null;
        }
    }

    public void Delete()
    {
        GameObject.Destroy(GameObject);
    }

    protected void Update_Material()
    {
        if (string.IsNullOrEmpty(Material) || !Material_Type.HasValue || !Active || MeshRenderer == null) {
            return;
        }
        MeshRenderer.material = MaterialManager.Instance.Get(Material, Material_Type.Value);
    }

    public class PrototypeData
    {
        public string Prefab_Name { get; set; }
        public string Material { get; set; }
        public MaterialManager.MaterialType? Material_Type { get; set; }
        public string Model_Name { get; set; }
        public bool Has_Collision { get; set; }
        public bool Use_Prefab { get { return !string.IsNullOrEmpty(Prefab_Name); } }

        public PrototypeData(string prefab_name, string material, MaterialManager.MaterialType? material_type)
        {
            Prefab_Name = prefab_name;
            Material = material;
            Material_Type = material_type;
            Model_Name = null;
            Has_Collision = false;
        }

        public PrototypeData(string model_name, bool has_collision)
        {
            Prefab_Name = null;
            Material = null;
            Material_Type = null;
            Model_Name = model_name;
            Has_Collision = has_collision;
        }
    }
}

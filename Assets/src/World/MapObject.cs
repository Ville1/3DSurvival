using UnityEngine;

public class MapObject {
    public string Name { get; protected set; }
    public GameObject GameObject { get; protected set; }
    public GameObject Parent { get; protected set; }
    public string Prefab_Name { get; protected set; }
    public string Material { get; protected set; }
    public MaterialManager.MaterialType Material_Type { get; protected set; }
    public string Model_Name { get; protected set; }

    public MapObject(string name, Vector3 position, GameObject parent, string prefab_name, string material, MaterialManager.MaterialType material_type, string model_name, bool active)
    {
        Name = name;
        Parent = parent;
        Prefab_Name = prefab_name;
        Material = material;
        Material_Type = material_type;
        Model_Name = model_name;

        GameObject = GameObject.Instantiate(
            !string.IsNullOrEmpty(prefab_name) ? PrefabManager.Instance.Get(prefab_name) : ModelManager.Instance.Get(model_name),
            position,
            Quaternion.identity,
            Parent.transform
        );
        GameObject.name = Name;
        GameObject.SetActive(active);
        Update_Material();
    }

    public MapObject(string name, string prefab_name, string material, MaterialManager.MaterialType material_type, string model_name)
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
        if (string.IsNullOrEmpty(Material) || !Active || MeshRenderer == null) {
            return;
        }
        MeshRenderer.material = MaterialManager.Instance.Get(Material, Material_Type);
    }
}

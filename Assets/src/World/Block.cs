using UnityEngine;

public class Block : MapObject {
    public static readonly string GAME_OBJECT_NAME_PREFIX = "Block_";
    public static readonly string PREFAB_NAME = "Block";

    private static long current_id = 0;

    public long Id { get; private set; }
    public string Internal_Name { get; private set; }
    public bool Passable { get; private set; }
    public bool Inactive_GameObject { get; private set; }
    public Coordinates Coordinates { get; private set; }

    private GameObject crack_cube;

    public Block(Coordinates position, Block prototype, GameObject container) : base(prototype.Name, position.Vector, container, PREFAB_NAME, prototype.Material, MaterialManager.MaterialType.Block,
        null, !prototype.Inactive_GameObject)
    {
        Id = current_id;
        current_id++;
        Coordinates = new Coordinates(position);
        Change_To(prototype, false);

        GameObject.name = string.Format("{0}#{1}_{2}", GAME_OBJECT_NAME_PREFIX, Id, Coordinates.Parse_Text(true, false));
        foreach(Transform transform in GameObject.transform) {
            if(transform.name == "CrackCube") {
                crack_cube = transform.gameObject;
            }
        }
        Update_Material();
    }

    public Block(string name, string internal_name, string material, bool passable, bool inactive) : base(name, PREFAB_NAME, material, MaterialManager.MaterialType.Block, null)
    {
        Id = -1;
        Name = name;
        Internal_Name = internal_name;
        Material = material;
        Passable = passable;
        Inactive_GameObject = inactive;
    }

    public static long? Parse_Id_From_GameObject_Name(string name)
    {
        if(!name.Contains(GAME_OBJECT_NAME_PREFIX) || !name.Contains("#")) {
            return null;
        }
        long id = 0;
        string sub = name.Substring(name.IndexOf("#") + 1);
        if (!long.TryParse(sub.Substring(0, sub.IndexOf("_")), out id)) {
            CustomLogger.Instance.Error(string.Format("Error parsing string: {0}", name));
            return null;
        }
        return id;
    }

    private void Change_To(Block prototype, bool update_material = true)
    {
        Name = prototype.Name;
        Internal_Name = prototype.Internal_Name;
        Material = prototype.Material;
        Passable = prototype.Passable;
        Inactive_GameObject = prototype.Inactive_GameObject;

        if (update_material) {
            Update_Material();
        }
    }

    private new void Update_Material()
    {
        base.Update_Material();
        crack_cube.SetActive(false);
    }
}

using UnityEngine;

public class ItemPile : Entity {

    public ItemPile(Vector3 position, ItemPile prototype, GameObject container, Inventory items) : base(position, prototype, container)
    {
        Inventory = items.Clone();
    }

    public ItemPile(string name, string prefab_name, string material, MaterialManager.MaterialType? material_type, string model_name) :
        base(name, prefab_name, material, material_type, model_name)
    {

    }

    public static ItemPile Prototype
    {
        get {
            return new ItemPile("Item pile", "Item_Pile", null, null, null);
        }
    }
}

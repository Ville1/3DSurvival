using System.Collections.Generic;
using UnityEngine;

public class CollisionBehaviour : MonoBehaviour {

    private List<long> block_ids;
    private List<long> standing_block_ids;
    private List<long> entity_ids;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start () {
        block_ids = new List<long>();
        standing_block_ids = new List<long>();
        entity_ids = new List<long>();
	}

    private void Update () { }

    private void OnCollisionEnter(Collision collision)
    {
        long? block_id = Block.Parse_Id_From_GameObject_Name(collision.gameObject.name);
        if (block_id.HasValue) {
            if (!block_ids.Contains(block_id.Value)) {
                block_ids.Add(block_id.Value);
            }
            if (!standing_block_ids.Contains(block_id.Value) && gameObject.transform.position.y > collision.gameObject.transform.position.y) {
                standing_block_ids.Add(block_id.Value);
            }
        }
        long? entity_id = Entity.Parse_Id_From_GameObject_Name(collision.gameObject.name);
        if (entity_id.HasValue && !entity_ids.Contains(entity_id.Value)) {
            entity_ids.Add(entity_id.Value);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        long? block_id = Block.Parse_Id_From_GameObject_Name(collision.gameObject.name);
        if (block_id.HasValue) {

            if (block_ids.Contains(block_id.Value)) {
                block_ids.Remove(block_id.Value);
            }
            if (standing_block_ids.Contains(block_id.Value)) {
                standing_block_ids.Remove(block_id.Value);
            }
        }
        long? entity_id = Entity.Parse_Id_From_GameObject_Name(collision.gameObject.name);
        if (entity_id.HasValue && entity_ids.Contains(entity_id.Value)) {
            entity_ids.Remove(entity_id.Value);
        }
    }

    public bool Block_Contact
    {
        get {
            return standing_block_ids != null && standing_block_ids.Count != 0;
        }
    }

    public List<Entity> Entities
    {
        get {
            if(entity_ids == null) {
                return new List<Entity>();
            }
            List<Entity> entities = new List<Entity>();
            foreach(long id in entity_ids) {
                Entity entity = Map.Instance.Get_Entity(id);
                if(entity != null) {
                    entities.Add(entity);
                }
            }
            return entities;
        }
    }
}

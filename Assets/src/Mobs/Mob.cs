using System.Diagnostics;
using UnityEngine;

public class Mob : Entity
{
    private static readonly float VERTICAL_MOVEMENT_FORCE_MULTIPLIER = 100.0f;
    private static readonly float HORIZONTAL_SPEED_MULTIPLIER = 5.0f;

    public float Movement_Speed { get; private set; }
    public float Jump_Strenght { get; private set; }

    public Direction Current_Movement;

    public Mob(Vector3 position, Mob prototype, GameObject container) : base(position, prototype, container)
    {
        Movement_Speed = prototype.Movement_Speed;
        Jump_Strenght = prototype.Jump_Strenght;
    }

    public Mob(string name, string prefab_name, string material, MaterialManager.MaterialType material_type, string model_name, float movement_speed, float jump_strenght) : base(name, prefab_name, material, material_type, model_name)
    {
        Movement_Speed = movement_speed;
        Jump_Strenght = jump_strenght;
    }

    public new void Update(float delta_time)
    {
        Stopwatch watch = Stopwatch.StartNew();
        base.Update(delta_time);
        delta_time += (watch.ElapsedMilliseconds * 0.001f);
        watch.Stop();

        if (!Collision_Data.Block_Contact) {
            Current_Movement = new Direction();
        }
        if (Current_Movement.Is_Empty && Collision_Data.Block_Contact) {
            Rigidbody.velocity = new Vector3(
                0.0f,
                Rigidbody.velocity.y,
                0.0f
            );
        } else if (!Current_Movement.Is_Empty) {
            Vector3 multipliers = new Vector3(
                GameObject.transform.forward.x * (int)Current_Movement.X,
                Current_Movement.Y_Multiplier,
                GameObject.transform.forward.z * (int)Current_Movement.Z
            );
            if (Current_Movement.Y == Direction.Shift.Positive) {
                Vector3 v = (GameObject.transform.position + (GameObject.transform.forward * (10000.0f * (int)Current_Movement.X)) + (GameObject.transform.right * (10000.0f * (int)Current_Movement.Z))).normalized;
                Rigidbody.AddForce(new Vector3(
                    Jump_Strenght * VERTICAL_MOVEMENT_FORCE_MULTIPLIER * v.x,
                    Jump_Strenght * VERTICAL_MOVEMENT_FORCE_MULTIPLIER * multipliers.y,
                    Jump_Strenght * VERTICAL_MOVEMENT_FORCE_MULTIPLIER * v.z
                ), ForceMode.Force);
            } else {
                GameObject.transform.position = Vector3.MoveTowards(
                    GameObject.transform.position,
                    GameObject.transform.position + (GameObject.transform.forward * (10000.0f * (int)Current_Movement.X)) + (GameObject.transform.right * (10000.0f * (int)Current_Movement.Z)),
                    Movement_Speed * delta_time * HORIZONTAL_SPEED_MULTIPLIER
                );
            }
        }
    }
}

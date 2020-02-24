using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class Mob : Entity
{
    private enum ActionType { Moving, Dismantle, Repair, Build, Craft }

    private static readonly float VERTICAL_MOVEMENT_FORCE_MULTIPLIER = 200.0f;
    private static readonly float HORIZONTAL_SPEED_MULTIPLIER = 2.5f;
    private static readonly string IDLE_TEXT = "Idle";
    private static readonly float OPERATION_RANGE = 4.0f;

    private static readonly string MESSAGE_INDESTRUCTIBLE = "Indestructible";
    private static readonly string MESSAGE_OBSTRUCTED = "Obstructed";
    private static readonly string MESSAGE_CANT_REACH = "Out of reach";
    private static readonly string MESSAGE_INSUFFICENT_SKILL = "Insufficent {0} skill, {1} required";
    private static readonly string MESSAGE_INSUFFICENT_ITEMS = "Not enough {0}, {1} required";
    private static readonly string MESSAGE_INSUFFICENT_TOOL = "Level {0} {1} required";
    private static readonly string MESSAGE_BUSY = "Busy: {0}";

    private static readonly string PROGRESS_TEXT = "{0} {1}%";

    public float HP { get; private set; }
    public int MAX_HP { get; private set; }
    public float Movement_Speed { get; private set; }
    public float Jump_Strenght { get; private set; }
    public Direction Current_Movement { get; set; }
    public List<Skill> Skills { get; private set; }
    public float Dismantling_Speed { get; private set; }
    public float Building_Speed { get; private set; }
    public bool Picks_Up_Items { get; private set; }

    private List<ActionData> actions;

    public Mob(Vector3 position, Mob prototype, GameObject container) : base(position, prototype, container)
    {
        Movement_Speed = prototype.Movement_Speed;
        Jump_Strenght = prototype.Jump_Strenght;
        MAX_HP = prototype.MAX_HP;
        HP = prototype.MAX_HP;
        Skills = new List<Skill>();
        foreach(Skill skill in prototype.Skills) {
            Skills.Add(new Skill(skill));
        }
        Current_Movement = new Direction();
        actions = new List<ActionData>();
        Dismantling_Speed = prototype.Dismantling_Speed;
        Building_Speed = prototype.Building_Speed;
        Picks_Up_Items = prototype.Picks_Up_Items;
    }

    public Mob(string name, string prefab_name, string material, MaterialManager.MaterialType? material_type, string model_name, float movement_speed, float jump_strenght, int hp, List<Skill> skills,
        float dismantling_speed, float building_speed, bool picks_up_items) :
        base(name, prefab_name, material, material_type, model_name)
    {
        MAX_HP = hp;
        HP = -1;
        Movement_Speed = movement_speed;
        Jump_Strenght = jump_strenght;
        Current_Movement = new Direction();
        actions = new List<ActionData>();
        Skills = Helper.Clone_List(skills);
        Dismantling_Speed = dismantling_speed;
        Building_Speed = building_speed;
        Picks_Up_Items = picks_up_items;
    }

    public new void Update(float delta_time)
    {
        Stopwatch watch = Stopwatch.StartNew();
        base.Update(delta_time);
        delta_time += (watch.ElapsedMilliseconds * 0.001f);
        watch.Stop();

        //Movement
        actions = actions.Where(x => x.Type != ActionType.Moving).ToList();
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
            if (Current_Movement.Y == Direction.Shift.Positive) {
                Vector3 v = Vector3.MoveTowards(
                    new Vector3(),
                    new Vector3() + (GameObject.transform.forward * (10000.0f * (int)Current_Movement.X)) + (GameObject.transform.right * (10000.0f * (int)Current_Movement.Z))
                     + (GameObject.transform.up * (10000.0f * (int)Current_Movement.Y)),
                    1.0f
                );
                Rigidbody.AddForce(new Vector3(
                    Jump_Strenght * VERTICAL_MOVEMENT_FORCE_MULTIPLIER * v.x,
                    Jump_Strenght * VERTICAL_MOVEMENT_FORCE_MULTIPLIER * v.y,
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
        if (!Current_Movement.Is_Empty) {
            actions.Add(new ActionData("Moving", "Moving", ActionType.Moving, false));
            List<ActionData> canceled_actions = new List<ActionData>();
            foreach(ActionData data in actions) {
                if (data.Moving_Cancels) {
                    canceled_actions.Add(data);
                    FloatingMessageManager.Instance.Show(string.Format("{0} canceled", data.Name));
                }
            }
            foreach(ActionData canceled_action in canceled_actions) {
                actions.Remove(canceled_action);
            }
        }

        //Dismantling
        ActionData dismantling = actions.FirstOrDefault(x => x.Type == ActionType.Dismantle);
        if(dismantling != null) {
            List<Tool> tools_used = new List<Tool>();
            float tool_efficency = Get_Tool_Efficency(dismantling.Target.Tools_Required_To_Dismantle, out tools_used);
            string verb = dismantling.Target.Dismantle_Verb.Present;
            if (!dismantling.Target.Deal_Damage(delta_time * Dismantling_Speed * tool_efficency, true)) {
                dismantling.Text = string.Format(PROGRESS_TEXT, string.Format("{0} {1}", dismantling.Target.Dismantle_Verb.Present, dismantling.Target.Name), Helper.Float_To_String(100.0f * (1.0f - dismantling.Target.Relative_HP), 0));
            } else {
                FloatingMessageManager.Instance.Show(string.Format("{0} finished", verb));
                dismantling.Finished = true;
            }
            //TODO: Tool durability
        }

        //Repairing
        ActionData repairing = actions.FirstOrDefault(x => x.Type == ActionType.Repair);
        if (repairing != null) {
            List<Tool> tools_used = new List<Tool>();
            float tool_efficency = Get_Tool_Efficency(repairing.Target.Tools_Required_To_Build, out tools_used);
            if (!repairing.Target.RepairOrBuild(delta_time * Building_Speed * tool_efficency)) {
                repairing.Text = string.Format(PROGRESS_TEXT, string.Format("Repairing {0}", repairing.Target.Name), Helper.Float_To_String(100.0f * repairing.Target.Relative_HP, 0));
            } else {
                FloatingMessageManager.Instance.Show("Repairing finished");
                repairing.Finished = true;
            }
            //TODO: Tool durability
        }

        //Building
        ActionData building = actions.FirstOrDefault(x => x.Type == ActionType.Build);
        if (building != null) {
            List<Tool> tools_used = new List<Tool>();
            float tool_efficency = Get_Tool_Efficency(building.Target.Tools_Required_To_Build, out tools_used);
            if (!building.Target.RepairOrBuild(delta_time * Building_Speed * tool_efficency)) {
                building.Text = string.Format(PROGRESS_TEXT, string.Format("Building {0}", building.Target.Name), Helper.Float_To_String(100.0f * building.Target.Relative_HP, 0));
            } else {
                FloatingMessageManager.Instance.Show("Building finished");
                building.Finished = true;
            }
            //TODO: Tool durability
        }

        //Clear actions
        List<ActionData> finished_actions = actions.Where(x => x.Finished).ToList();
        foreach(ActionData finished_action in finished_actions) {
            actions.Remove(finished_action);
        }

        //Pick up items
        foreach(Entity entity in Collision_Data.Entities) {
            if(!(entity is ItemPile)) {
                continue;
            }
            ItemPile pile = (entity as ItemPile);
            Inventory added_items = new Inventory();
            foreach(Item item in pile.Inventory) {
                if (Inventory.Can_Fit(item)) {
                    Inventory.Add(item);
                    added_items.Add(item);
                }
            }
            foreach(Item item in added_items) {
                pile.Inventory.Remove(item);
            }
            if (pile.Inventory.Is_Empty) {
                pile.Delete();
                Map.Instance.Remove_Entity(pile);
            }
            if (!added_items.Is_Empty) {
                FloatingMessageManager.Instance.Show(added_items.Parse_Text(true));
            }
        }
    }

    public string Current_Action_Text
    {
        get {
            ActionData data = actions.FirstOrDefault(x => x.Type != ActionType.Moving);
            if(data != null) {
                return data.Text;
            }
            data = actions.FirstOrDefault();
            return data != null ? data.Text : IDLE_TEXT;
        }
    }

    public bool Dismantle_Block(Block block, out string message)
    {
        message = null;
        if (block.Indestructible) {
            message = MESSAGE_INDESTRUCTIBLE;
            return false;
        }
        if (!Can_Work(out message)) {
            return false;
        }
        if (!Can_Operate(block)) {
            message = MESSAGE_CANT_REACH;
            return false;
        }
        if(!Has_Skills(block.Skills_Required_To_Dismantle, out message)) {
            return false;
        }
        if(!Has_Tools(block.Tools_Required_To_Dismantle, out message)) {
            return false;
        }
        actions.Add(new ActionData(block.Dismantle_Verb.Present, string.Format(PROGRESS_TEXT, string.Format("{0} {1}", block.Dismantle_Verb.Present, block.Name),
            Helper.Float_To_String(100.0f * (1.0f - block.Relative_HP), 0)), ActionType.Dismantle, true, block));
        return true;
    }

    public bool Repair_Block(Block block, out string message)
    {
        message = null;
        if (block.Indestructible) {
            message = MESSAGE_INDESTRUCTIBLE;
            return false;
        }
        if (!Can_Work(out message)) {
            return false;
        }
        if (!Can_Operate(block)) {
            message = MESSAGE_CANT_REACH;
            return false;
        }
        if (!Has_Skills(block.Skills_Required_To_Build, out message)) {
            return false;
        }
        if (!Has_Tools(block.Tools_Required_To_Build, out message)) {
            return false;
        }
        actions.Add(new ActionData("Repair", string.Format(PROGRESS_TEXT, string.Format("Repairing {0}", block.Name), Helper.Float_To_String(100.0f * block.Relative_HP, 0)),
            ActionType.Repair, true, block));
        return true;
    }

    public bool Build_Block(Block prototype, Block target, out string message)
    {
        message = null;
        //TODO: Check for map objects in location
        //TODO: Duplicated code
        if (!target.Can_Be_Built_Over) {
            message = MESSAGE_OBSTRUCTED;
            return false;
        }
        if (!Can_Operate(target)) {
            message = MESSAGE_CANT_REACH;
            return false;
        }
        if (!Can_Build(prototype, out message)) {
            return false;
        }
        foreach(KeyValuePair<string, int> resource in prototype.Materials_Required_To_Build) {
            for(int i = 0; i < resource.Value; i++) {
                Inventory.Remove(resource.Key);
            }
        }
        target.Change_To(prototype, true, 0.01f);
        actions.Add(new ActionData("Build", string.Format(PROGRESS_TEXT, string.Format("Building {0}", prototype.Name), Helper.Float_To_String(100.0f * target.Relative_HP, 0)),
            ActionType.Build, true, target));
        return true;
    }

    public bool Can_Build(Block prototype, Block location, out string message)
    {
        message = null;
        //TODO: Check for map objects in location
        if (!location.Can_Be_Built_Over) {
            message = MESSAGE_OBSTRUCTED;
            return false;
        }
        if (!Can_Operate(location)) {
            message = MESSAGE_CANT_REACH;
            return false;
        }
        if (!Can_Build(prototype, out message)) {
            return false;
        }
        return true;
    }

    public bool Can_Build(Block prototype, out string message)
    {
        message = null;
        if (!Can_Work(out message)) {
            return false;
        }
        if (!Has_Skills(prototype.Skills_Required_To_Build, out message)) {
            return false;
        }
        if (!Has_Tools(prototype.Tools_Required_To_Build, out message)) {
            return false;
        }
        if (!Has_Items(prototype.Materials_Required_To_Build, out message)) {
            return false;
        }
        return true;
    }

    public bool Can_Operate(Block block)
    {
        return Vector3.Distance(block.Position, Position) <= OPERATION_RANGE;
    }

    public bool Has_Skills(Dictionary<Skill.SkillId, int> required, out string message)
    {
        message = null;
        foreach(KeyValuePair<Skill.SkillId, int> requirement in required) {
            if(!Has_Skill(requirement.Key, requirement.Value)) {
                message = string.Format(MESSAGE_INSUFFICENT_SKILL, requirement.Key, requirement.Value);
                return false;
            }
        }
        return true;
    }

    public bool Has_Skill(Skill.SkillId id, int level)
    {
        return Skills.Exists(x => x.Id == id && x.Level >= level);
    }

    public bool Has_Items(Dictionary<string, int> items, out string message)
    {
        message = null;
        foreach(KeyValuePair<string, int> data in items) {
            if(!Inventory.Has_Items(data.Key, data.Value)) {
                message = string.Format(MESSAGE_INSUFFICENT_ITEMS, ItemPrototypes.Instance.Get_Item(data.Key).Name, data.Value);
                return false;
            }
        }
        return true;
    }

    public bool Can_Work(out string message)
    {
        message = null;
        if(actions.Count != 0) {
            message = string.Format(MESSAGE_BUSY, actions[0].Name);
            return false;
        }
        return true;
    }

    public bool Has_Tools(Dictionary<Tool.ToolType, int> required, out string message)
    {
        message = null;
        foreach (KeyValuePair<Tool.ToolType, int> requirement in required) {
            if (!Inventory.Has_Tool(requirement.Key, requirement.Value)) {
                message = string.Format(MESSAGE_INSUFFICENT_TOOL, requirement.Value, requirement.Key);
                return false;
            }
        }
        return true;
    }

    public float Get_Tool_Efficency(Dictionary<Tool.ToolType, int> data, out List<Tool> tools)
    {
        tools = new List<Tool>();
        if (data.Count == 0) {
            return 1.0f;
        }
        float total = 0.0f;
        float count = 0.0f;
        foreach(KeyValuePair<Tool.ToolType, int> tool_data in data) {
            Tool tool = Inventory.Get_Tool(tool_data.Key, tool_data.Value);
            if(tool != null) {
                total += tool.Efficiency;
                tools.Add(tool);
            }
            count += 1.0f;
        }
        return total / count;
    }

    private class ActionData
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public ActionType Type { get; set; }
        public Block Target { get; set; }
        public bool Finished { get; set; }
        public bool Moving_Cancels { get; set; }

        public ActionData(string name, string text, ActionType type, bool moving_cancels)
        {
            Name = name;
            Text = text;
            Type = type;
            Moving_Cancels = moving_cancels;
            Target = null;
            Finished = false;
        }

        public ActionData(string name, string text, ActionType type, bool moving_cancels, Block target)
        {
            Name = name;
            Text = text;
            Type = type;
            Moving_Cancels = moving_cancels;
            Target = target;
            Finished = false;
        }
    }
}

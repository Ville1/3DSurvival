﻿using System.Collections.Generic;
using UnityEngine;

public class Block : MapObject {
    public static readonly string GAME_OBJECT_NAME_PREFIX = "Block_";
    public static readonly string TRANSPARENT_MATERIAL_PREFIX = "transparent_";
    public static readonly string PREFAB_NAME = "Block";

    private static long current_id = 0;
    private static readonly string[] CRACK_MATERIALS = new string[3] { "cracks_1", "cracks_2", "cracks_3" };

    public long Id { get; private set; }
    public string Internal_Name { get; private set; }
    public bool Passable { get; private set; }
    public bool Inactive_GameObject { get; private set; }
    public bool Indestructible { get; private set; }
    public bool Can_Be_Built_Over { get; private set; }
    public int MAX_HP { get; private set; }
    public float HP { get; private set; }
    public float HP_Dismantled { get; private set; }
    public float HP_Built { get; private set; }
    public string UI_Sprite { get; private set; }
    public SpriteManager.SpriteType UI_Sprite_Type { get; private set; }
    public float Dismantle_Speed { get; private set; }
    public float Build_Speed { get; private set; }
    public Dictionary<string, int> Dismantle_Drops { get; private set; }
    public Dictionary<string, int> Materials_Required_To_Build { get; private set; }
    public Dictionary<Skill.SkillId, int> Skills_Required_To_Dismantle { get; private set; }
    public Dictionary<Skill.SkillId, int> Skills_Required_To_Build { get; private set; }
    public Dictionary<Tool.ToolType, int> Tools_Required_To_Dismantle { get; private set; }
    public Dictionary<Tool.ToolType, int> Tools_Required_To_Build { get; private set; }
    public Verb Dismantle_Verb { get; private set; }
    public float Relative_HP { get { return Indestructible ? 1.0f : HP / MAX_HP; } }
    public bool Completed { get; private set; }
    public BuildMenuManager.TabType? Build_Menu_Tab { get; private set; }
    public bool Buildable { get { return Build_Menu_Tab.HasValue; } }
    public bool Preview { get; private set; }

    private GameObject crack_cube;

    public Block(Coordinates position, Block prototype, GameObject container, float? hp = null, bool is_preview = false) : base(prototype.Name, position.Vector, container, PREFAB_NAME, prototype.Material, MaterialManager.MaterialType.Block,
        null, !prototype.Inactive_GameObject)
    {
        Id = current_id;
        current_id++;
        Change_To(prototype, false, hp, is_preview);

        GameObject.name = string.Format("{0}#{1}_{2}", GAME_OBJECT_NAME_PREFIX, Id, Coordinates.Parse_Text(true, false));
        foreach(Transform transform in GameObject.transform) {
            if(transform.name == "CrackCube") {
                crack_cube = transform.gameObject;
            }
        }
        Map.Instance.Add_Block(this);
        Update_Material();
        if (is_preview) {
            GameObject.GetComponentInChildren<BoxCollider>().enabled = false;
        }
    }

    public Block(string name, string internal_name, string material, bool passable, bool can_be_built_over, bool inactive, int hp, string ui_sprite, SpriteManager.SpriteType ui_sprite_type, float dismantle_speed,
        float build_speed, Dictionary<string, int> dismantle_drops, Dictionary<string, int> building_materials, Dictionary<Skill.SkillId, int> dismantle_skills, Dictionary<Skill.SkillId, int> build_skills,
        Verb dismantle_verb, Dictionary<Tool.ToolType, int> tools_required_to_dismantle, Dictionary<Tool.ToolType, int> tools_required_to_build, BuildMenuManager.TabType? build_menu_tab) : 
        base(name, PREFAB_NAME, material, MaterialManager.MaterialType.Block, null)
    {
        Id = -1;
        Name = name;
        Internal_Name = internal_name;
        Material = material;
        Passable = passable;
        Can_Be_Built_Over = can_be_built_over;
        Inactive_GameObject = inactive;
        MAX_HP = hp;
        Indestructible = hp <= 0.0f;
        HP = -1.0f;
        HP_Dismantled = -1.0f;
        HP_Built = -1.0f;
        UI_Sprite = ui_sprite;
        UI_Sprite_Type = ui_sprite_type;
        Dismantle_Speed = dismantle_speed;
        Build_Speed = build_speed;
        Dismantle_Drops = dismantle_drops != null ? Helper.Clone_Dictionary(dismantle_drops) : new Dictionary<string, int>();
        Materials_Required_To_Build = building_materials != null ? Helper.Clone_Dictionary(building_materials) : new Dictionary<string, int>();
        Skills_Required_To_Dismantle = dismantle_skills != null ? Helper.Clone_Dictionary(dismantle_skills) : new Dictionary<Skill.SkillId, int>();
        Skills_Required_To_Build = build_skills != null ? Helper.Clone_Dictionary(build_skills) : new Dictionary<Skill.SkillId, int>();
        Dismantle_Verb = new Verb(dismantle_verb);
        Tools_Required_To_Dismantle = tools_required_to_dismantle != null ? Helper.Clone_Dictionary(tools_required_to_dismantle) : new Dictionary<Tool.ToolType, int>();
        Tools_Required_To_Build = tools_required_to_build != null ? Helper.Clone_Dictionary(tools_required_to_build) : new Dictionary<Tool.ToolType, int>();
        Completed = true;
        Build_Menu_Tab = build_menu_tab;
    }

    public Coordinates Coordinates
    {
        get {
            return new Coordinates(Position);
        }
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

    public void Change_To(Block prototype, bool update_material = true, float? hp = null, bool is_preview = false)
    {
        Name = prototype.Name;
        Internal_Name = prototype.Internal_Name;
        Material = prototype.Material;
        Passable = prototype.Passable;
        Can_Be_Built_Over = prototype.Can_Be_Built_Over;
        Inactive_GameObject = prototype.Inactive_GameObject;
        HP = hp.HasValue ? hp.Value : prototype.MAX_HP;
        MAX_HP = prototype.MAX_HP;
        HP_Dismantled = 0.0f;
        HP_Built = 0.0f;
        Indestructible = prototype.Indestructible;
        UI_Sprite = prototype.UI_Sprite;
        UI_Sprite_Type = prototype.UI_Sprite_Type;
        Dismantle_Speed = prototype.Dismantle_Speed;
        Build_Speed = prototype.Build_Speed;
        Dismantle_Drops = Helper.Clone_Dictionary(prototype.Dismantle_Drops);
        Materials_Required_To_Build = Helper.Clone_Dictionary(prototype.Materials_Required_To_Build);
        Skills_Required_To_Dismantle = Helper.Clone_Dictionary(prototype.Skills_Required_To_Dismantle);
        Skills_Required_To_Build = Helper.Clone_Dictionary(prototype.Skills_Required_To_Build);
        Dismantle_Verb = new Verb(prototype.Dismantle_Verb);
        Tools_Required_To_Build = Helper.Clone_Dictionary(prototype.Tools_Required_To_Build);
        Tools_Required_To_Dismantle = Helper.Clone_Dictionary(prototype.Tools_Required_To_Dismantle);
        Completed = HP == MAX_HP;
        Build_Menu_Tab = prototype.Build_Menu_Tab;
        Preview = is_preview;

        GameObject.SetActive(!Inactive_GameObject);
        if (update_material) {
            Update_Material();
        }
    }

    private new void Update_Material()
    {
        base.Update_Material();
        crack_cube.SetActive(false);
        if (Completed && Relative_HP < 1.0f) {
            crack_cube.SetActive(true);
            string material_name = CRACK_MATERIALS[Mathf.RoundToInt((CRACK_MATERIALS.Length - 1) * (1.0f - Relative_HP))];
            MeshRenderer renderer = crack_cube.GetComponentInChildren<MeshRenderer>();
            if (!renderer.material.name.StartsWith(material_name)) {
                renderer.material = MaterialManager.Instance.Get(material_name, MaterialManager.MaterialType.Block);
            }
        }
        if((Preview || !Completed) && !MeshRenderer.material.name.StartsWith(TRANSPARENT_MATERIAL_PREFIX)) {
            MeshRenderer.material = MaterialManager.Instance.Get(TRANSPARENT_MATERIAL_PREFIX + Material, MaterialManager.MaterialType.Block);
        } else if (!Preview && Completed && MeshRenderer.material.name.StartsWith(TRANSPARENT_MATERIAL_PREFIX)) {
            MeshRenderer.material = MaterialManager.Instance.Get(Material, MaterialManager.MaterialType.Block);
        }
    }

    public bool Deal_Damage(float amount, bool dismantle)
    {
        HP = Mathf.Max(0.0f, HP - amount);
        if (dismantle) {
            HP_Dismantled += amount;
        }
        bool broke = HP == 0.0f;
        if (broke) {
            //TODO: 0.95f
            if (Completed && HP_Dismantled >= 0.95f * MAX_HP && Dismantle_Drops.Count != 0) {
                ItemPile pile = new ItemPile(Position, ItemPile.Prototype, Map.Instance.Entity_Container, new Inventory(Dismantle_Drops));
            } else if(!Completed && HP_Dismantled >= 0.95f * HP_Built && Materials_Required_To_Build.Count != 0) {
                ItemPile pile = new ItemPile(Position, ItemPile.Prototype, Map.Instance.Entity_Container, new Inventory(Materials_Required_To_Build));
            }
            Change_To(BlockPrototypes.Instance.Air);
        }
        Update_Material();
        return broke;
    }

    public bool RepairOrBuild(float amount)
    {
        HP = Mathf.Min(MAX_HP, HP + amount);
        HP_Dismantled = Mathf.Max(0.0f, HP_Dismantled - amount);
        if(!Completed) {
            HP_Built = Mathf.Min(HP, HP_Built + amount);
            if (HP == MAX_HP) {
                Completed = true;
            }
        }
        Update_Material();
        return HP == MAX_HP;
    }

    public new void Delete()
    {
        base.Delete();
        Map.Instance.Remove_Block(this);
    }

    public override string ToString()
    {
        return string.Format("{0} block #{1} {2}", Internal_Name, Id, Coordinates.Parse_Text(true, true));
    }
}

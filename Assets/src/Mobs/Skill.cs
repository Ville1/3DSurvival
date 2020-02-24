public class Skill {
    public enum SkillId { Mining, Masonry }

    public string Name { get; private set; }
    public SkillId Id { get; private set; }
    public int Level { get; private set; }
    public float Experience { get; private set; }

    public Skill(Skill prototype)
    {
        Name = prototype.Name;
        Id = prototype.Id;
        Level = prototype.Level < 0 ? 0 : prototype.Level;
        Experience = 0.0f;
    }

    public Skill(string name, SkillId id)
    {
        Name = name;
        Id = id;
        Level = -1;
        Experience = -1;
    }

    public Skill(string name, SkillId id, int starting_level)
    {
        Name = name;
        Id = id;
        Level = starting_level;
        Experience = -1;
    }
}

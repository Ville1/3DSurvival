public class Verb {
    public string Base { get; private set; }
    public string Present { get; private set; }

    public Verb(Verb prototype)
    {
        Base = prototype.Base;
        Present = prototype.Present;
    }

    public Verb(string base_p, string present)
    {
        Base = base_p;
        Present = present;
    }
}

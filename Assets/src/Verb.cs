public class Verb {
    private static readonly string PLACEHOLDER_STRING = "???";

    public string Base { get; private set; }
    public string Present { get; private set; }

    public Verb(Verb prototype)
    {
        if (prototype == null) {
            Base = PLACEHOLDER_STRING;
            Present = PLACEHOLDER_STRING;
        } else {
            Base = prototype.Base;
            Present = prototype.Present;
        }
    }

    public Verb(string base_p, string present)
    {
        Base = base_p;
        Present = present;
    }
}

using ProtoBuf;

[ProtoContract]
public class Morpheme : ISynchronizable
{
    [ProtoMember(1)]
    public string Meaning;
    [ProtoMember(2)]
    public string Value;

    [ProtoMember(3)]
    public WordType Type;

    [ProtoMember(4)]
    public int PropertiesInt;

    public MorphemeProperties Properties;

    public Morpheme()
    {
    }

    public void Synchronize()
    {
        PropertiesInt = (int)Properties;
    }

    public void FinalizeLoad()
    {
        Properties = (MorphemeProperties)PropertiesInt;
    }
}

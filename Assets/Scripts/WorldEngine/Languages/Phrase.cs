using ProtoBuf;

[ProtoContract]
public class Phrase : ISynchronizable
{
    [ProtoMember(1)]
    public string Original;
    [ProtoMember(2)]
    public string Meaning;
    [ProtoMember(3)]
    public string Text;

    [ProtoMember(4)]
    public int PropertiesInt;

    public PhraseProperties Properties;

    public void Synchronize()
    {
        PropertiesInt = (int)Properties;
    }

    public void FinalizeLoad()
    {
        Properties = (PhraseProperties)PropertiesInt;
    }

    public Phrase()
    {
    }

    public Phrase(Phrase phrase)
    {
        Original = phrase.Original;
        Meaning = phrase.Meaning;
        Text = phrase.Text;

        Properties = phrase.Properties;
    }
}

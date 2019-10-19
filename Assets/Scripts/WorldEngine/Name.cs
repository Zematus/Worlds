using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProtoBuf;

public class Variation
{
    public string Tags;
    public string Text;

    public Variation(string text)
    {
        Text = text;
        Tags = string.Empty;
    }

    public Variation(string text, string tags)
    {
        Text = text;
        Tags = tags;
    }
}

public static class NameTools
{
    public static Regex OptionalWordPartRegex = new Regex(@"\{(?:\<(?<tags>\w+)\>)?(?<word>.+?)\}");
    public static Regex NameRankRegex = new Regex(@"\[\[(?<rank>.+?)\]\](?<word>.+?)");

    public static Variation[] GenerateNounVariations(string[] variants)
    {
        List<Variation> variations = new List<Variation>();

        foreach (string variant in variants)
        {
            GenerateNounVariations(variant, variations);
        }

        return variations.ToArray();
    }

    public static void GenerateNounVariations(string variant, List<Variation> variations)
    {
        Match match = NameTools.OptionalWordPartRegex.Match(variant);

        if (!match.Success)
        {
            variations.Add(new Variation(variant));
            return;
        }

        string v1Str = variant.Replace(match.Value, string.Empty);
        string v2Str = variant.Replace(match.Value, match.Groups["word"].Value);

        Variation v1 = new Variation(v1Str);
        Variation v2 = new Variation(v2Str);

        if (match.Groups["tags"].Success)
        {
            if (string.IsNullOrEmpty(v2.Tags))
                v2.Tags = match.Groups["tags"].Value;
            else
                v2.Tags += "," + match.Groups["tags"].Value;
        }

        GenerateNounVariations(v1, variations);
        GenerateNounVariations(v2, variations);
    }

    public static void GenerateNounVariations(Variation variation, List<Variation> variations)
    {
        Match match = NameTools.OptionalWordPartRegex.Match(variation.Text);

        if (!match.Success)
        {
            variations.Add(variation);
            return;
        }

        string v1Str = variation.Text.Replace(match.Value, string.Empty);
        string v2Str = variation.Text.Replace(match.Value, match.Groups["word"].Value);

        Variation v1 = new Variation(v1Str, variation.Tags);
        Variation v2 = new Variation(v2Str, variation.Tags);

        if (match.Groups["tags"].Success)
        {
            if (string.IsNullOrEmpty(v2.Tags))
                v2.Tags = match.Groups["tags"].Value;
            else
                v2.Tags += "," + match.Groups["tags"].Value;
        }

        GenerateNounVariations(v1, variations);
        GenerateNounVariations(v2, variations);
    }
}

[ProtoContract]
public class Name : ISynchronizable
{
    [ProtoMember(1)]
    public long LanguageId;

    [ProtoMember(2)]
    public string TaggedMeaning;

    public World World;

    public Language Language;

    public Phrase Value => _value ?? (_value = Language.TranslatePhrase(TaggedMeaning));

    private Phrase _value = null;

    public Name()
    {

    }

    public Name(string taggedMeaning, Language language, World world)
    {
        World = world;

        LanguageId = language.Id;
        Language = language;

        TaggedMeaning = taggedMeaning;
    }

    public string BoldText => Value.Text.ToBoldFormat();

    public string Text => Value.Text;

    public string Meaning => Value.Meaning;


    public void Synchronize()
    {
    }

    public void FinalizeLoad()
    {
        Language = World.GetLanguage(LanguageId);

        if (Language == null)
        {
            throw new System.Exception("Language can't be null");
        }
    }

    public override string ToString()
    {
        return Value.Text + " (" + Value.Meaning + ")";
    }
}

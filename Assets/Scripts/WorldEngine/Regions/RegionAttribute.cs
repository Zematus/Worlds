using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class RegionAttribute
{
    public const string RelationTag = "relation";

    public string Name;

    public string[] Adjectives;

    public Variation[] Variations;

    public RegionConstraint[] Constraints;

    public Association[] Associations;

    public bool Secondary;

    public static Dictionary<string, RegionAttribute> Attributes;
    public static Dictionary<string, RegionAttribute> SecondaryAttributes;

    public RegionAttribute(string name, string[] adjectives, string[] variants, string[] constraints, string[] associationStrs, bool secondary = false)
    {
        Name = name;

        if (adjectives == null)
        {
            Adjectives = new string[] { };
        }
        else
        {
            Adjectives = adjectives;
        }

        Variations = NameTools.GenerateNounVariations(variants);

        if (constraints != null)
        {
            Constraints = new RegionConstraint[constraints.Length];

            int index = 0;
            foreach (string constraint in constraints)
            {
                Constraints[index] = RegionConstraint.BuildConstraint(constraint);
                index++;
            }
        }
        else
        {
            Constraints = new RegionConstraint[] { };
        }

        List<Association> associations = new List<Association>();

        foreach (string assocStr in associationStrs)
        {
            associations.AddRange(Association.Parse(assocStr));
        }

        Associations = associations.ToArray();

        Secondary = secondary;
    }

    public string GetRandomVariation(GetRandomIntDelegate getRandomInt, Element filterElement = null, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations;

        if (filterElement != null)
        {
            filteredVariations = Variations.Where(v => !v.Text.Contains(filterElement.SingularName));
        }

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomVariation(GetRandomIntDelegate getRandomInt, string filterStr, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations;

        filterStr = filterStr.ToLower();

        if (filterStr != null)
        {
            filteredVariations = Variations.Where(v => !v.Text.Contains(filterStr));
        }

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomSingularVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations.Where(v => !Language.IsPluralForm(v.Text));

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomPluralVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations.Where(v => Language.IsPluralForm(v.Text));

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged)
    {
        IEnumerable<Variation> filteredVariations = Variations;

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public bool Assignable(Region region)
    {
        foreach (RegionConstraint c in Constraints)
        {
            if (!c.Validate(region)) return false;
        }

        return true;
    }

    public static void ResetAttributes()
    {
        Attributes = new Dictionary<string, RegionAttribute>();
        SecondaryAttributes = new Dictionary<string, RegionAttribute>();
    }

    private static void AddAttribute(Dictionary<string, RegionAttribute> attributes, RegionAttribute attribute)
    {
        if (attributes.ContainsKey(attribute.Name))
        {
            attributes[attribute.Name] = attribute;
        }
        else
        {
            attributes.Add(attribute.Name, attribute);
        }
    }

    public static void LoadRegionAttributesFile(string filename)
    {
        foreach (RegionAttribute attribute in RegionAttributeLoader.Load(filename))
        {
            if (attribute.Secondary)
            {
                AddAttribute(SecondaryAttributes, attribute);
                continue;
            }

            AddAttribute(Attributes, attribute);
        }
    }
}

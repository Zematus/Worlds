using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class RegionAttribute
{
    public class Instance
    {
        public RegionAttribute RegionAttribute;

        public List<string> Adjectives = new List<string>();

        public string Id { get { return RegionAttribute.Id; } }
        public string Name { get { return RegionAttribute.Name; } }

        public Association[] Associations { get { return RegionAttribute.Associations; } }

        public bool Secondary { get { return RegionAttribute.Secondary; } }

        public string GetRandomVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
        {
            return RegionAttribute.GetRandomVariation(getRandomInt, filterRelationTagged);
        }

        public string GetRandomSingularVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
        {
            return RegionAttribute.GetRandomSingularVariation(getRandomInt, filterRelationTagged);
        }

        public string GetRandomPluralVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
        {
            return RegionAttribute.GetRandomPluralVariation(getRandomInt, filterRelationTagged);
        }
    }

    public const string RelationTag = "relation";

    public string Id;

    public string Name;

    public Adjective[] Adjectives;

    public Variation[] Variations;

    public RegionConstraint[] Constraints;

    public Association[] Associations;

    public bool Secondary;

    public static Dictionary<string, RegionAttribute> Attributes;
    public static Dictionary<string, RegionAttribute> SecondaryAttributes;

    public RegionAttribute(string id, string name, Adjective[] adjectives, string[] variants, string[] constraints, string[] associationStrs, bool secondary = false)
    {
        Id = id;
        Name = name;

        if (adjectives == null)
        {
            Adjectives = new Adjective[] { };
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
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomSingularVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations.Where(v => !Language.IsPluralForm(v.Text));

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RelationTag));
        }
        
        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomPluralVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations.Where(v => Language.IsPluralForm(v.Text));

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RelationTag));
        }

        if (filteredVariations.Count() == 0)
        {
            string variations = string.Join(", ", Variations.Select(v => v.Text));

            throw new System.Exception(
                "No regional attribute variation with plural form within available set of variations: " + variations);
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged)
    {
        IEnumerable<Variation> filteredVariations = Variations;

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public Instance GetInstanceForRegion(Region region)
    {
        Instance instance = new Instance()
        {
            RegionAttribute = this
        };

        foreach (Adjective adj in Adjectives)
        {
            if (adj.Assignable(region))
            {
                instance.Adjectives.Add(adj.Word);
            }
        }

        return instance;
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
        if (attributes.ContainsKey(attribute.Id))
        {
            attributes[attribute.Id] = attribute;
        }
        else
        {
            attributes.Add(attribute.Id, attribute);
        }
    }

    public static void LoadRegionAttributesFile033(string filename)
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

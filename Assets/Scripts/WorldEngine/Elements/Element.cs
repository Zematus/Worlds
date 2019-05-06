using System.Collections.Generic;
using System.Linq;

public class Element
{
    public string Id;

    public string SingularName;
    public string PluralName;

    public string[] Adjectives;

    public Association[] Associations;

    public RegionConstraint[] Constraints;
    
    public static Dictionary<string, Element> Elements;

    public Element(string id, string pluralName, string[] adjectives, string[] constraints, string[] associationStrs)
    {
        Id = id;

        SingularName = Language.GetSingularForm(pluralName);
        PluralName = pluralName;

        if (adjectives != null)
        {
            Adjectives = adjectives;
        }
        else
        {
            Adjectives = new string[] { };
        }

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
    }

    public bool Assignable(Region region)
    {
        foreach (RegionConstraint c in Constraints)
        {
            if (!c.Validate(region)) return false;
        }

        return true;
    }

    public static void LoadFile(string filename)
    {
        Elements = new Dictionary<string, Element>();

        foreach (Element element in ElementLoader.Load(filename))
        {
            if (Elements.ContainsKey(element.Id))
            {
                throw new System.Exception("duplicate element id: " + element.Id);
            }

            Elements.Add(element.Id, element);
        }

        if (Elements.Count == 0)
        {
            throw new System.Exception("No elements loaded from " + filename);
        }
    }
}

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

    public static void ResetElements()
    {
        Elements = new Dictionary<string, Element>();
    }

    public static void LoadElementsFile(string filename)
    {
        foreach (Element element in ElementLoader.Load(filename))
        {
            if (Elements.ContainsKey(element.Id))
            {
                Elements[element.Id] = element;
            }
            else
            {
                Elements.Add(element.Id, element);
            }
        }
    }
}

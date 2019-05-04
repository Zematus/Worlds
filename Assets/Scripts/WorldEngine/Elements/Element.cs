using System.Collections.Generic;
using System.Linq;

public class Element
{
    public string Id;

    public string SingularName;
    public string PluralName;

    public string[] Adjectives;

    public Association[] Associations;

    public ElementConstraint[] Constraints;
    
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
            Constraints = new ElementConstraint[constraints.Length];

            int index = 0;
            foreach (string constraint in constraints)
            {
                Constraints[index] = ElementConstraint.BuildConstraint(constraint);
                index++;
            }
        }
        else
        {
            Constraints = new ElementConstraint[] { };
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
        foreach (ElementConstraint c in Constraints)
        {
            if (!c.Validate(region)) return false;
        }

        return true;
    }
}

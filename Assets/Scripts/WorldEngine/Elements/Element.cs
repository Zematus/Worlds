using System.Collections.Generic;
using System.Linq;

public class Element
{
    public class Instance
    {
        public Element Element;

        public List<string> Adjectives = new List<string>();

        public string Id { get { return Element.Id; } }
        public string SingularName { get { return Element.SingularName; } }
        public string PluralName { get { return Element.PluralName; } }

        public Association[] Associations { get { return Element.Associations; } }
    }

    public string Id;

    public string SingularName;
    public string PluralName;

    public Adjective[] Adjectives;

    public Association[] Associations;

    public RegionConstraint[] Constraints;
    
    public static Dictionary<string, Element> Elements;

    public Element(string id, string pluralName, Adjective[] adjectives, string[] constraints, string[] associationStrs)
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
            Adjectives = new Adjective[] { };
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

    public Instance GetInstanceForRegion(Region region)
    {
        Instance instance = new Instance()
        {
            Element = this
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

    public static void ResetElements()
    {
        Elements = new Dictionary<string, Element>();
    }

    public static void LoadElementsFile033(string filename)
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

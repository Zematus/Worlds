using System.Collections.Generic;
using System.Linq;

public class Adjective
{
    public string Id;
    
    public string Word;
    
    public RegionConstraint[] Constraints;
    
    public static Dictionary<string, Adjective> Adjectives;

    public Adjective(string id, string word, string[] constraints = null)
    {
        Id = id;
        
        Word = word;

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
    }

    public bool Assignable(Region region)
    {
        foreach (RegionConstraint c in Constraints)
        {
            if (!c.Validate(region)) return false;
        }

        return true;
    }

    public static void ResetAdjectives()
    {
        Adjectives = new Dictionary<string, Adjective>();
    }

    public static void LoadAdjectivesFile033(string filename)
    {
        foreach (Adjective adjective in AdjectiveLoader.Load(filename))
        {
            if (Adjectives.ContainsKey(adjective.Id))
            {
                Adjectives[adjective.Id] = adjective;
            }
            else
            {
                Adjectives.Add(adjective.Id, adjective);
            }
        }
    }

    public static Adjective TryGetAdjectiveOrAdd(string adj)
    {
        Adjective adjective;

        if (!Adjectives.TryGetValue(adj, out adjective))
        {
            adjective = new Adjective(adj, adj);

            Adjectives.Add(adjective.Id, adjective);
        }

        return adjective;
    }
}

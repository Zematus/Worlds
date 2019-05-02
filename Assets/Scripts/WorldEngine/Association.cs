using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class AssociationForms
{
    public const string NameSingular = "ns";
    public const string DefiniteSingular = "ds";
    public const string IndefiniteSingular = "is";
    public const string DefinitePlural = "dp";
    public const string IndefinitePlural = "ip";
    public const string Uncountable = "u";
}

public class Association
{
    public static Regex AssocDefRegex = new Regex(@"^(?<noun>(?:\[[^\[\]]+\])*(?:\w+\:?)+)(?:,(?<relations>(?:\w+\|?)+),(?<forms>(?:\w+\|?)+))?$");

    public string Noun;
    public bool IsAdjunction;

    public string Relation;
    public string Form;

    public Association(string noun)
    {
        Noun = noun;
        IsAdjunction = true;

        Relation = null;
        Form = null;
    }

    public Association(string noun, string relation, string form)
    {
        Noun = noun;
        IsAdjunction = false;

        Relation = relation;
        Form = form;
    }

    public static Association[] Parse(string associationStr)
    {
        Match match = Association.AssocDefRegex.Match(associationStr);

        if (!match.Success)
        {
            throw new System.Exception("Association string not valid: " + associationStr);
        }

        string noun = match.Groups["noun"].Value;

        bool isAdjunction = string.IsNullOrEmpty(match.Groups["relations"].Value);

        if (isAdjunction)
        {
            return new Association[] { new Association(noun) };
        }

        string[] relations = match.Groups["relations"].Value.Split('|');
        string[] forms = match.Groups["forms"].Value.Split('|');

        Association[] associations = new Association[1 + (relations.Length * forms.Length)];

        int index = 0;
        associations[index++] = new Association(noun);

        foreach (string relation in relations)
        {
            foreach (string form in forms)
            {
                associations[index++] = new Association(noun, relation, form);
            }
        }

        return associations;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

public static class QuotedStringListHelper
{
    public const string FirstAndLastSingleQuoteRegex = @"(?:^\s*\'\s*)|(?:\s*\'\s*$)";
    public const string SeparatorSingleQuoteRegex = @"\s*(?:(?:\'\s*,\s*\'))\s*";
}

[Serializable]
public class ElementLoader
{

#pragma warning disable 0649

    public LoadedElement[] elements;

    [Serializable]
    public class LoadedElement
    {
        public string id;
        public string name;
        public string adjectives;
        public string regionConstraints;
        public string phraseAssociations;
    }

#pragma warning restore 0649

    public static IEnumerable<Element> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);
        
        ElementLoader loader = JsonUtility.FromJson<ElementLoader>(jsonStr);
        
        for (int i = 0; i < loader.elements.Length; i++)
        {
            yield return CreateElement(loader.elements[i]);
        }
    }

    private static Element CreateElement(LoadedElement e)
    {
        if (string.IsNullOrEmpty(e.id))
        {
            throw new ArgumentException("element id can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.name))
        {
            throw new ArgumentException("element name can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.phraseAssociations))
        {
            throw new ArgumentException("element's phrase association strings can't be null or empty");
        }

        Adjective[] adjectives = null;
        string[] constraints = null;
        string[] associationStrs = null;

        if (!string.IsNullOrEmpty(e.adjectives))
        {
            string[] adjs = e.adjectives.Split(',');
            adjectives = new Adjective[adjs.Length];

            for (int i = 0; i < adjs.Length; i++)
            {
                string adj = adjs[i].Trim();

                adjectives[i] = Adjective.TryGetAdjectiveOrAdd(adj);
            }
        }

        if (!string.IsNullOrEmpty(e.regionConstraints))
        {
            //Cleanup and split list of constraints
            string c = Regex.Replace(e.regionConstraints, QuotedStringListHelper.FirstAndLastSingleQuoteRegex, "");
            constraints = Regex.Split(c, QuotedStringListHelper.SeparatorSingleQuoteRegex);
        }

        //Cleanup and split list of association strings
        string a = Regex.Replace(e.phraseAssociations, QuotedStringListHelper.FirstAndLastSingleQuoteRegex, "");
        associationStrs = Regex.Split(a, QuotedStringListHelper.SeparatorSingleQuoteRegex);

        Element element = new Element(e.id, e.name, adjectives, constraints, associationStrs);

        return element;
    }
}

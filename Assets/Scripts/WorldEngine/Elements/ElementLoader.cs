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
        public string constraints;
        public string associationStrings;
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

        if (string.IsNullOrEmpty(e.associationStrings))
        {
            throw new ArgumentException("element's association strings can't be null or empty");
        }

        string[] adjectives = null;
        string[] constraints = null;
        string[] associationStrs = null;

        if (!string.IsNullOrEmpty(e.adjectives))
        {
            adjectives = e.adjectives.Split(',');

            for (int i = 0; i < adjectives.Length; i++)
            {
                adjectives[i] = adjectives[i].Trim();
            }
        }

        if (!string.IsNullOrEmpty(e.constraints))
        {
            //Cleanup and split list of constraints
            string c = Regex.Replace(e.constraints, QuotedStringListHelper.FirstAndLastSingleQuoteRegex, "");
            constraints = Regex.Split(c, QuotedStringListHelper.SeparatorSingleQuoteRegex);
        }

        //Cleanup and split list of association strings
        string a = Regex.Replace(e.associationStrings, QuotedStringListHelper.FirstAndLastSingleQuoteRegex, "");
        associationStrs = Regex.Split(a, QuotedStringListHelper.SeparatorSingleQuoteRegex);

        Element element = new Element(e.id, e.name, adjectives, constraints, associationStrs);

        return element;
    }
}

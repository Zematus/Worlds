using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

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
            throw new ArgumentException("element's associationStrings can't be null or empty");
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
            string c = Regex.Replace(e.constraints, @"^\s*\'\s*", "");
            c = Regex.Replace(c, @"\s*\'\s*$", "");
            constraints = Regex.Split(c, @"\s*(?:(?:\'\s*,\s*\'))\s*");

            for (int i = 0; i < constraints.Length; i++)
            {
                Debug.Log("constraints[" + i + "]: " + constraints[i]);
            }
        }

        //Cleanup and split list of association strings
        string a = Regex.Replace(e.associationStrings, @"^\s*\'\s*", "");
        a = Regex.Replace(a, @"\s*\'\s*$", "");
        associationStrs = Regex.Split(a, @"\s*(?:(?:\'\s*,\s*\'))\s*");

        for (int i = 0; i < associationStrs.Length; i++)
        {
            Debug.Log("associationStrs[" + i + "]: " + associationStrs[i]);
        }

        Element element = new Element(e.id, e.name, adjectives, constraints, associationStrs);

        return element;
    }
}

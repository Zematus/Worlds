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
        public string[] adjectives;
        public string[] regionConstraints;
        public string[] phraseAssociations;
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

        if (e.phraseAssociations == null)
        {
            throw new ArgumentException("element's phrase association strings can't be null");
        }

        Adjective[] adjectives = null;

        if (e.adjectives != null)
        {
            adjectives = new Adjective[e.adjectives.Length];

            for (int i = 0; i < e.adjectives.Length; i++)
            {
                string adj = e.adjectives[i].Trim();

                adjectives[i] = Adjective.TryGetAdjectiveOrAdd(adj);
            }
        }

        Element element = new Element(e.id, e.name, adjectives, e.regionConstraints, e.phraseAssociations);

        return element;
    }
}

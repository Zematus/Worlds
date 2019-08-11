using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

[Serializable]
public class RegionAttributeLoader
{

#pragma warning disable 0649

    public LoadedRegionAttribute[] regionAttributes;

    [Serializable]
    public class LoadedRegionAttribute
    {
        public string id;
        public string name;
        public string[] adjectives;
        public string[] variants;
        public string[] regionConstraints;
        public string[] phraseAssociations;
        public bool secondary;
    }

#pragma warning restore 0649

    public static IEnumerable<RegionAttribute> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        RegionAttributeLoader loader = JsonUtility.FromJson<RegionAttributeLoader>(jsonStr);
        
        for (int i = 0; i < loader.regionAttributes.Length; i++)
        {
            yield return CreateRegionAttribute(loader.regionAttributes[i]);
        }
    }

    private static RegionAttribute CreateRegionAttribute(LoadedRegionAttribute attr)
    {
        if (string.IsNullOrEmpty(attr.id))
        {
            throw new ArgumentException("region attribute id can't be null or empty");
        }

        if (string.IsNullOrEmpty(attr.name))
        {
            throw new ArgumentException("region attribute name can't be null or empty");
        }

        if (attr.variants == null)
        {
            throw new ArgumentException("region attribute's variants can't be null");
        }

        if (attr.phraseAssociations == null)
        {
            throw new ArgumentException("region phrase attribute's association strings can't be null");
        }

        Adjective[] adjectives = null;
        string[] variants = null;

        if (attr.adjectives != null)
        {
            adjectives = new Adjective[attr.adjectives.Length];

            for (int i = 0; i < attr.adjectives.Length; i++)
            {
                string adj = attr.adjectives[i].Trim();

                adjectives[i] = Adjective.TryGetAdjectiveOrAdd(adj);
            }
        }

        variants = attr.variants;

        for (int i = 0; i < variants.Length; i++)
        {
            variants[i] = variants[i].Trim();
        }

        RegionAttribute regionAttribute = new RegionAttribute(attr.id, attr.name, adjectives, variants, attr.regionConstraints, attr.phraseAssociations, attr.secondary);

        return regionAttribute;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

[Serializable]
public class AdjectiveLoader
{

#pragma warning disable 0649

    public LoadedAdjective[] adjectives;

    [Serializable]
    public class LoadedAdjective
    {
        public string id;
        public string word;
        public string[] regionConstraints;
    }

#pragma warning restore 0649

    public static IEnumerable<Adjective> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);
        
        AdjectiveLoader loader = JsonUtility.FromJson<AdjectiveLoader>(jsonStr);
        
        for (int i = 0; i < loader.adjectives.Length; i++)
        {
            yield return CreateAdjective(loader.adjectives[i]);
        }
    }

    private static Adjective CreateAdjective(LoadedAdjective adj)
    {
        if (string.IsNullOrEmpty(adj.id))
        {
            throw new ArgumentException("adjective id can't be null or empty");
        }

        if (string.IsNullOrEmpty(adj.word))
        {
            throw new ArgumentException("adjective word can't be null or empty");
        }
        
        Adjective adjective = new Adjective(adj.id, adj.word, adj.regionConstraints);

        return adjective;
    }
}

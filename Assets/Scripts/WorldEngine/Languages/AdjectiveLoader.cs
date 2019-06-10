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
        public string regionConstraints;
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
        
        string[] constraints = null;

        if (!string.IsNullOrEmpty(adj.regionConstraints))
        {
            //Cleanup and split list of constraints
            string c = Regex.Replace(adj.regionConstraints, ModUtility.FirstAndLastSingleQuoteRegex, "");
            constraints = Regex.Split(c, ModUtility.SeparatorSingleQuoteRegex);
        }
        
        Adjective adjective = new Adjective(adj.id, adj.word, constraints);

        return adjective;
    }
}

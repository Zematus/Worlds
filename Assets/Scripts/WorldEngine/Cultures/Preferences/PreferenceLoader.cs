using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

[Serializable]
public class PreferenceLoader
{
#pragma warning disable 0649

    public LoadedPreference[] preferences;

    [Serializable]
    public class LoadedPreference
    {
        public string id;
        public string name;
    }

#pragma warning restore 0649

    public static IEnumerable<PreferenceGenerator> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        PreferenceLoader loader = JsonUtility.FromJson<PreferenceLoader>(jsonStr);

        for (int i = 0; i < loader.preferences.Length; i++)
        {
            yield return CreatePreferenceGenerator(loader.preferences[i]);
        }
    }

    private static PreferenceGenerator CreatePreferenceGenerator(LoadedPreference p)
    {
        if (string.IsNullOrEmpty(p.id))
        {
            throw new ArgumentException("discovery id can't be null or empty");
        }

        if (string.IsNullOrEmpty(p.name))
        {
            throw new ArgumentException("discovery name can't be null or empty");
        }

        PreferenceGenerator preferenceGenerator = new PreferenceGenerator()
        {
            Id = p.id,
            IdHash = p.id.GetHashCode(),
            Name = p.name,
        };

        return preferenceGenerator;
    }
}

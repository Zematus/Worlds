using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PreferenceGenerator
{
    public static Dictionary<string, PreferenceGenerator> Generators;

    public string Id;
    public string Name;

    public int IdHash;

    public static void ResetPreferenceGenerators()
    {
        Generators = new Dictionary<string, PreferenceGenerator>();
    }

    public static void LoadPreferencesFile(string filename)
    {
        foreach (PreferenceGenerator preference in PreferenceLoader.Load(filename))
        {
            if (Generators.ContainsKey(preference.Id))
            {
                Generators[preference.Id] = preference;
            }
            else
            {
                Generators.Add(preference.Id, preference);
            }
        }
    }

    public static void InitializePreferenceGenerators()
    {
        foreach (PreferenceGenerator generator in Generators.Values)
        {
            generator.Initialize();
        }
    }

    public static PreferenceGenerator GetGenerator(string id)
    {
        if (!Generators.TryGetValue(id, out PreferenceGenerator p))
        {
            return null;
        }

        return p;
    }

    public void Initialize()
    {
        World.PreferenceGenerators.Add(Id, this);
    }
}

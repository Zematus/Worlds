using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DiscoveryClass
{
    public static Dictionary<string, DiscoveryClass> Discoveries;

    public string Id;
    public string Name;

    public int IdHash;

    public Condition[] GainConditions = null;
    public Condition[] HoldConditions = null;

    public Effect[] GainEffects = null;
    public Effect[] LossEffects = null;

    public long EventTimeToTrigger;
    public Factor[] EventTimeToTriggerFactors = null;

    public bool IsPresentAtStart = false;

    public static void ResetDiscoveries()
    {
        Discoveries = new Dictionary<string, DiscoveryClass>();
    }

    public static void LoadDiscoveriesFile(string filename)
    {
        foreach (DiscoveryClass discovery in DiscoveryLoader.Load(filename))
        {
            if (Discoveries.ContainsKey(discovery.Id))
            {
                Discoveries[discovery.Id] = discovery;
            }
            else
            {
                Discoveries.Add(discovery.Id, discovery);
            }
        }
    }
}

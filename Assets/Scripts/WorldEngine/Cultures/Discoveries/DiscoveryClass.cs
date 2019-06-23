using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DiscoveryClass : ICellGroupEventGenerator
{
    public static Dictionary<string, DiscoveryClass> Discoveries;

    public string Id;
    public string Name;

    public int IdHash;
    public string EventId;
    public string EventSetFlag;

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

    public static void InitializeDiscoveries()
    {
        foreach (DiscoveryClass discovery in Discoveries.Values)
        {
            discovery.Initialize();
        }
    }

    public void Initialize()
    {
        EventId = Id + "_discovery_event";
        EventSetFlag = EventId + "_set";
    }

    public bool CanGainDiscovery(CellGroup group)
    {
        if (group.Culture.HasDiscovery(Id))
            return false;

        foreach (Condition condition in GainConditions)
        {
            if (!condition.Evaluate(group))
                return false;
        }

        return true;
    }

    public bool CanAssignEventTypeToGroup(CellGroup group)
    {
        if (group.IsFlagSet(EventSetFlag))
            return false;

        return CanGainDiscovery(group);
    }

    public CellGroupEvent GenerateAndAddEvent(CellGroup group)
    {
        throw new System.NotImplementedException();
    }
}

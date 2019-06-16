using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DiscoveryClass
{
    public string Id;
    public string Name;

    public int IdHash;
    
    public long EventTimeToTrigger;

    public Condition[] GainConditions = null;
    public Condition[] HoldConditions = null;

    public Effect[] GainEffects = null;
    public Effect[] LossEffects = null;

    public Effect[] TimeToTriggerFactors = null;
}

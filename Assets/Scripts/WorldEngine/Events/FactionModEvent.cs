using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FactionModEvent : ModEvent
{
    public Faction TargetFaction;

    public FactionModEvent(Faction targetFaction, EventGenerator generator)
        : base(generator)
    {
        TargetFaction = targetFaction;
    }
}

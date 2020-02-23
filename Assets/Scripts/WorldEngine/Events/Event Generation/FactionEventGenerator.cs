using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FactionEventGenerator : EventGenerator
{
    private readonly FactionEntity _target;

    public FactionEventGenerator(string targetStr)
    {
        _target = new FactionEntity(targetStr);

        // Add the target to the context's entity map
        Entities.Add(targetStr, _target);
    }

    public override ModEvent GenerateEvent()
    {
        FactionModEvent modEvent = new FactionModEvent(_target.Faction, this);

        return modEvent;
    }

    public void SetTargetFaction(Faction target)
    {
        _target.Set(target);
    }

    protected override float GetNextRandomFloat(int seed)
    {
        return _target.Faction.GetNextLocalRandomFloat(seed);
    }
}

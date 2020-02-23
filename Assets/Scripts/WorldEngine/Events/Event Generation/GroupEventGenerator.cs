using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class GroupEventGenerator : EventGenerator
{
    private readonly GroupEntity _target;

    public GroupEventGenerator(string targetStr)
    {
        _target = new GroupEntity(targetStr);

        // Add the target to the context's entity map
        Entities.Add(targetStr, _target);
    }

    public override ModEvent GenerateEvent(long triggerDate)
    {
        GroupModEvent modEvent = new GroupModEvent(_target.Group, this, triggerDate);

        return modEvent;
    }

    public void SetTargetGroup(CellGroup target)
    {
        _target.Set(target);
    }

    public override long GenerateUniqueIdentifier(long triggerDate)
    {
        CellGroup group = _target.Group;

        if (triggerDate > World.MaxSupportedDate)
        {
            Debug.LogWarning("GroupEventGenerator.GenerateUniqueIdentifier - 'triggerDate' is greater than " + World.MaxSupportedDate + " (triggerDate = " + triggerDate + ")");
        }

        long id = (triggerDate * 1000000000L) + (group.Longitude * 1000000L) + (group.Latitude * 1000L) + IdHash;

        return id;
    }

    protected override float GetNextRandomFloat(int seed)
    {
        return _target.Group.GetNextLocalRandomFloat(seed);
    }
}

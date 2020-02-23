using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class GroupModEvent : ModEvent
{
    public CellGroup TargetGroup;

    public GroupModEvent(
        CellGroup group,
        EventGenerator generator,
        long triggerDate)
        : base(generator, group.World, triggerDate)
    {
        TargetGroup = group;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class ModEvent : WorldEvent
{
    protected EventGenerator _generator;

    public ModEvent(
        EventGenerator generator,
        World world,
        long triggerDate)
        : base(
            world,
            triggerDate,
            generator.GenerateUniqueIdentifier(triggerDate),
            generator.IdHash)
    {
        _generator = generator;
    }

    public override void Trigger()
    {
        throw new System.NotImplementedException();
    }
}

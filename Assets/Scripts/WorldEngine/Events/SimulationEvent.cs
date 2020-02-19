using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class SimulationEvent : WorldEvent
{
    public EventContext Context;

    public EventGenerator Generator;

    public override void Trigger()
    {
        throw new System.NotImplementedException();
    }
}

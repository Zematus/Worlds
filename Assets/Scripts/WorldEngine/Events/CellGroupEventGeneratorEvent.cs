using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellGroupEventGeneratorEvent : CellGroupEvent
{
    [XmlAttribute("GId")]
    public string GeneratorId;

    [XmlIgnore]
    public ICellGroupEventGenerator Generator;

    public CellGroupEventGeneratorEvent()
    {
    }

    public CellGroupEventGeneratorEvent(
        ICellGroupEventGenerator generator, 
        CellGroup group, 
        long triggerDate, 
        long eventTypeId) : 
        base(group, triggerDate, eventTypeId)
    {
        Generator = generator;
        GeneratorId = generator.GetEventGeneratorId();
    }

    public override bool CanTrigger()
    {
        return Generator.CanTriggerEvent(Group);
    }

    public override void Trigger()
    {
        Generator.TriggerEvent(Group);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        Generator = World.GetGenerator(GeneratorId) as ICellGroupEventGenerator;

        if (Generator == null)
        {
            throw new System.Exception("CellGroupEventGeneratorEvent: Generator with Id:" + GeneratorId + " not found");
        }
    }
}

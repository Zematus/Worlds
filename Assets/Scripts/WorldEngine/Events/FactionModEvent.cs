using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FactionModEvent : FactionEvent
{
    [XmlAttribute("GenId")]
    public string GeneratorId;

    private FactionEventGenerator _generator;

    [XmlIgnore]
    public string EventSetFlag;

    public FactionModEvent(
        FactionEventGenerator generator,
        Faction faction,
        long triggerDate)
        : base(faction, triggerDate, generator.IdHash)
    {
        _generator = generator;

        GeneratorId = generator.Id;
        EventSetFlag = generator.EventSetFlag;

        faction.SetFlag(EventSetFlag);
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
        {
            return false;
        }

        _generator.SetTarget(Faction);

        if (!_generator.CanTriggerEvent())
        {
            return false;
        }

        return true;
    }

    public override void Trigger()
    {
        _generator.SetTarget(Faction);

        _generator.TriggerEvent();
    }

    protected override void DestroyInternal()
    {
        if (_generator.TryReasignEvent(this))
        {
            // If reasigned then we don't need to fully destroy the event
            return;
        }

        if (Faction != null)
        {
            Faction.UnsetFlag(EventSetFlag);
        }

        base.DestroyInternal();
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _generator = EventGenerator.GetGenerator(GeneratorId) as FactionEventGenerator;

        if (_generator == null)
        {
            throw new System.Exception(
                "FactionModEvent: Generator with Id:" + GeneratorId + " not found");
        }
    }
}

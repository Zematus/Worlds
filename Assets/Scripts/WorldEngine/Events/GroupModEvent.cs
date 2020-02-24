using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class GroupModEvent : CellGroupEvent
{
    [XmlAttribute("GenId")]
    public string GeneratorId;

    private GroupEventGenerator _generator;

    public GroupModEvent(
        GroupEventGenerator generator,
        CellGroup group,
        long triggerDate)
        : base(group, triggerDate, generator.IdHash)
    {
        _generator = generator;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
        {
            return false;
        }

        _generator.SetTarget(Group);

        if (!_generator.CanTriggerEvent())
        {
            return false;
        }

        return true;
    }

    public override void Trigger()
    {
        _generator.SetTarget(Group);

        _generator.TriggerEvent();
    }

    protected override void DestroyInternal()
    {
        base.DestroyInternal();

        _generator.TryReasignEvent(this);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _generator = EventGenerator.GetGenerator(GeneratorId) as GroupEventGenerator;

        if (_generator == null)
        {
            throw new System.Exception(
                "GroupModEvent: Generator with Id:" + GeneratorId + " not found");
        }
    }
}

using System.Xml;
using System.Xml.Serialization;

public class CellGroupModEvent : CellGroupEvent
{
    [XmlAttribute("GenId")]
    public string GeneratorId;

    private CellGroupEventGenerator _generator;

    [XmlIgnore]
    public string EventSetFlag;

    public CellGroupModEvent()
    {
    }

    public CellGroupModEvent(
        CellGroupEventGenerator generator,
        CellGroup group,
        long triggerDate)
        : base(group, triggerDate, generator.IdHash)
    {
        _generator = generator;

        GeneratorId = generator.Id;
        EventSetFlag = generator.EventSetFlag;

        group.SetFlag(EventSetFlag);
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
        {
            return false;
        }

        _generator.SetTarget(Group);

        if (!_generator.CanTriggerEvent(this))
        {
            return false;
        }

        return true;
    }

    public override void Trigger()
    {
        // This operation assumes that CanTrigger() has been called beforehand,
        // and within, _generator.SetTarget(Group)...

        _generator.TriggerEvent(this);
    }

    protected override void DestroyInternal()
    {
        if (_generator.TryReasignEvent(this))
        {
            // If reasigned then we don't need to fully destroy the event
            return;
        }

        base.DestroyInternal();
    }

    public override void Cleanup()
    {
        if (Group != null)
        {
            Group.UnsetFlag(EventSetFlag);
        }

        base.Cleanup();
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _generator = EventGenerator.GetGenerator(GeneratorId) as CellGroupEventGenerator;
        EventSetFlag = _generator.EventSetFlag;

        if (_generator == null)
        {
            throw new System.Exception(
                "GroupModEvent: Generator with Id:" + GeneratorId + " not found");
        }
    }
}

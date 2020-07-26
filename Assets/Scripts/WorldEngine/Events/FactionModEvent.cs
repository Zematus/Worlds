using System.Xml;
using System.Xml.Serialization;

public class FactionModEvent : FactionEvent
{
    [XmlAttribute("GenId")]
    public string GeneratorId;

    [XmlIgnore]
    public FactionEventGenerator Generator;

    [XmlIgnore]
    public string EventSetFlag;

    public FactionModEvent()
    {
    }

    public FactionModEvent(
        FactionEventGenerator generator,
        Faction faction,
        long triggerDate)
        : base(faction, triggerDate, generator.IdHash)
    {
        Generator = generator;

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

        Generator.SetTarget(Faction);

        if (!Generator.CanTriggerEvent())
        {
            return false;
        }

        return true;
    }

    public override void Trigger()
    {
        // This operation assumes that CanTrigger() has been called beforehand,
        // and within, _generator.SetTarget(Faction)...

        Generator.TriggerEvent();
    }

    protected override void DestroyInternal()
    {
        if (Generator.TryReasignEvent(this))
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

        Generator = EventGenerator.GetGenerator(GeneratorId) as FactionEventGenerator;
        EventSetFlag = Generator.EventSetFlag;

        if (Generator == null)
        {
            throw new System.Exception(
                "FactionModEvent: Generator with Id:" + GeneratorId + " not found");
        }
    }
}

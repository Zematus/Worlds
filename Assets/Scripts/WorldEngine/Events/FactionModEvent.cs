using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

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
        Profiler.BeginSample("FactionModEvent - CanTrigger");

        if (!base.CanTrigger())
        {
            Profiler.EndSample(); // "FactionModEvent CanTrigger"

            return false;
        }

        Profiler.BeginSample("FactionModEvent - CanTrigger - SetTarget");

        Generator.SetTarget(Faction);

        Profiler.EndSample(); // "FactionModEvent CanTrigger - SetTarget"

        if (!Generator.CanTriggerEvent(this))
        {
            Profiler.EndSample(); // "FactionModEvent CanTrigger"

            return false;
        }

        Profiler.EndSample(); // "FactionModEvent CanTrigger"

        return true;
    }

    public override void Trigger()
    {
        // This operation assumes that CanTrigger() has been called beforehand,
        // and within, _generator.SetTarget(Faction)...

        Generator.TriggerEvent(this);
    }

    protected override void DestroyInternal()
    {
        if (Generator.TryReasignEvent(this))
        {
            // If reasigned then we don't need to fully destroy the event
            return;
        }

        base.DestroyInternal();
    }

    public override void Cleanup()
    {
        if (Faction != null)
        {
            Faction.UnsetFlag(EventSetFlag);
        }

        base.Cleanup();
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

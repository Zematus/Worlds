using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(Discovery.Event))]
public abstract class CellGroupEventGeneratorEvent : CellGroupEvent
{
    [ProtoMember(1)]
    public string GeneratorId;

    public ICellGroupEventGenerator Generator;

    public string EventSetFlag;

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
        EventSetFlag = generator.GetEventSetFlag();

        group.SetFlag(EventSetFlag);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        Generator = World.GetEventGenerator(GeneratorId) as ICellGroupEventGenerator;

        if (Generator == null)
        {
            throw new System.Exception("CellGroupEventGeneratorEvent: Generator with Id:" + GeneratorId + " not found");
        }
    }

    protected override void DestroyInternal()
    {
        Group?.UnsetFlag(EventSetFlag);

        base.DestroyInternal();
    }
}

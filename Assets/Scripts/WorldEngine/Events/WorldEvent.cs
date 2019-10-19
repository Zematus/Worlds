using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(UpdateCellGroupEvent))]
[ProtoInclude(200, typeof(MigrateGroupEvent))]
[ProtoInclude(300, typeof(ExpandPolityProminenceEvent))]
[ProtoInclude(400, typeof(TribeFormationEvent))]
[ProtoInclude(500, typeof(ClanSplitDecisionEvent))]
[ProtoInclude(600, typeof(TribeSplitDecisionEvent))]
[ProtoInclude(700, typeof(ClanDemandsInfluenceDecisionEvent))]
[ProtoInclude(800, typeof(ClanCoreMigrationEvent))]
[ProtoInclude(900, typeof(FosterTribeRelationDecisionEvent))]
[ProtoInclude(1000, typeof(MergeTribesDecisionEvent))]
[ProtoInclude(1100, typeof(OpenTribeDecisionEvent))]
[ProtoInclude(1200, typeof(Discovery.Event))]
[ProtoInclude(1300, typeof(CellEvent))]
[ProtoInclude(1400, typeof(CellGroupEvent))]
[ProtoInclude(1500, typeof(FactionEvent))]
[ProtoInclude(1600, typeof(PolityEvent))]
public abstract class WorldEvent : ISynchronizable
{
    public const long UpdateCellGroupEventId = 0;
    public const long MigrateGroupEventId = 1;
    public const long SailingDiscoveryEventId = 200;
    public const long TribalismDiscoveryEventId = 300;
    public const long TribeFormationEventId = 4;
    public const long BoatMakingDiscoveryEventId = 500;
    public const long PlantCultivationDiscoveryEventId = 600;

    public const long ClanSplitDecisionEventId = 7;
    public const long PreventClanSplitEventId = 8;

    public const long ExpandPolityProminenceEventId = 9;

    public const long TribeSplitDecisionEventId = 10;
    public const long SplitClanPreventTribeSplitEventId = 11;
    public const long PreventTribeSplitEventId = 12;

    public const long PolityFormationEventId = 13;
    public const long ClanCoreMigrationEventId = 14;

    public const long ClanDemandsInfluenceDecisionEventId = 15;
    public const long ClanAvoidsInfluenceDemandDecisionEventId = 16;
    public const long RejectInfluenceDemandDecisionEventId = 17;
    public const long AcceptInfluenceDemandDecisionEventId = 18;

    public const long FosterTribeRelationDecisionEventId = 20;
    public const long AvoidFosterTribeRelationDecisionEventId = 21;
    public const long RejectFosterTribeRelationDecisionEventId = 22;
    public const long AcceptFosterTribeRelationDecisionEventId = 23;

    public const long MergeTribesDecisionEventId = 25;
    public const long AvoidMergeTribesAttemptDecisionEventId = 26;
    public const long RejectMergeTribesOfferDecisionEventId = 27;
    public const long AcceptMergeTribesOfferDecisionEventId = 28;

    public const long OpenTribeDecisionEventId = 30;
    public const long AvoidOpenTribeDecisionEventId = 31;

    public World World;

    public bool DoNotSerialize = false;

    public bool FailedToTrigger = false;

    public BinaryTreeNode<long, WorldEvent> Node = null;

    [ProtoMember(1)]
    public long TypeId;

    [ProtoMember(2)]
    public long TriggerDate;

    [ProtoMember(3)]
    public long SpawnDate;

    [ProtoMember(4)]
    public long Id;

    public WorldEvent()
    {
        Manager.UpdateWorldLoadTrackEventCount();
    }

    public WorldEvent(World world, WorldEventData data, long id)
    {
        TypeId = data.TypeId;

        World = world;
        TriggerDate = data.TriggerDate;
        SpawnDate = data.SpawnDate;

        Id = id;
    }

    public WorldEvent(World world, long triggerDate, long id, long typeId, long originalSpawnDate = -1)
    {
        TypeId = typeId;

        World = world;
        TriggerDate = triggerDate;

        // This is used for some events recreated after load
        SpawnDate = originalSpawnDate > -1 ? originalSpawnDate : World.CurrentDate;

        Id = id;

#if DEBUG
        if ((Manager.RegisterDebugEvent == null) || (Manager.TracingData.Priority > 0))
            return;

        if (this.GetType().IsSubclassOf(typeof(CellGroupEvent)))
            return;

        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("WorldEvent - Id: " + id + ", Type: " + this.GetType(), 
            "TriggerDate: " + TriggerDate, SpawnDate);

        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
#endif
    }

    public WorldEvent(World world, long id, WorldEventData data) : this(world, data.TriggerDate, id, data.TypeId)
    {

    }

    public void AssociateNode(BinaryTreeNode<long, WorldEvent> node)
    {
        if (Node != null)
        {
            Node.Valid = false;
        }

        Node = node;
    }

    public virtual WorldEventData GetData()
    {
        return new WorldEventData(this);
    }

    public virtual WorldEventSnapshot GetSnapshot()
    {
        return new WorldEventSnapshot(this);
    }

    public virtual bool IsStillValid()
    {
        return true;
    }

    public virtual bool CanTrigger()
    {
        return IsStillValid();
    }

    public virtual void Synchronize()
    {

    }

    public virtual void FinalizeLoad()
    {

    }

    public abstract void Trigger();

    public void Destroy()
    {
        DestroyInternal();
    }

    protected virtual void DestroyInternal()
    {

    }

    public virtual void Reset(long newTriggerDate, long newId)
    {
        TriggerDate = newTriggerDate;
        SpawnDate = World.CurrentDate;
        Id = newId;

        FailedToTrigger = false;
    }
}

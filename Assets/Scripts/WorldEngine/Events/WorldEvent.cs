using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System;

public abstract class WorldEvent : ISynchronizable, IEffectTrigger
{
    public const long UpdateCellGroupEventId = 0;
    public const long MigrateGroupEventId = 1;
    public const long SailingDiscoveryEventId = 200;
    public const long TribalismDiscoveryEventId = 300;
    public const long TribeFormationEventId = 4;
    public const long BoatMakingDiscoveryEventId = 500;
    public const long PlantCultivationDiscoveryEventId = 600;

    public const long TribeSplitDecisionEventId = 10;
    public const long SplitClanPreventTribeSplitEventId = 11;
    public const long PreventTribeSplitEventId = 12;

    public const long PolityFormationEventId = 13;

    public const long MergeTribesDecisionEventId = 25;
    public const long AvoidMergeTribesAttemptDecisionEventId = 26;
    public const long RejectMergeTribesOfferDecisionEventId = 27;
    public const long AcceptMergeTribesOfferDecisionEventId = 28;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public bool DoNotSerialize = false;

    [XmlIgnore]
    public bool FailedToTrigger = false;

    [XmlIgnore]
    public BinaryTreeNode<long, WorldEvent> Node = null;

    [XmlAttribute("TId")]
    public long TypeId;

    [XmlAttribute("TD")]
    public long TriggerDate;

    [XmlAttribute("SD")]
    public long SpawnDate;

    // This Id doesn't need to be unique. but it helps if it is.
    [XmlAttribute]
    public long Id;

#if DEBUG
    [XmlIgnore]
    public long PrevId = -1;
#endif

#if DEBUG
    private Dictionary<IEffectExpression, long> _lastUseDates = new Dictionary<IEffectExpression, long>();

    public long GetLastUseDate(IEffectExpression expression)
    {
        if (_lastUseDates.ContainsKey(expression))
        {
            return _lastUseDates[expression];
        }

        return -1;
    }

    public void SetLastUseDate(IEffectExpression expression, long date)
    {
        _lastUseDates[expression] = date;
    }
#endif

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

        if (originalSpawnDate > -1)
            SpawnDate = originalSpawnDate; // This is used for some events recreated after load
        else
            SpawnDate = World.CurrentDate;

        Id = id;

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            if (!this.GetType().IsSubclassOf(typeof(CellGroupEvent)))
            {
                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("WorldEvent - Id: " + id + ", Type: " + this.GetType(), 
                    "TriggerDate: " + TriggerDate, SpawnDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
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

    /// <summary>
    /// Check if the event can be triggered under the current circumstances
    /// </summary>
    /// <returns>'true' if the event can be triggered, otherwise 'false'</returns>
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

    /// <summary>
    /// Trigger all the effects of this event
    /// </summary>
    public abstract void Trigger();

    public void Destroy()
    {
        DestroyInternal();
    }

    /// <summary>
    /// Performs subclass-specific cleanup tasks before destroying the object
    /// </summary>
    protected virtual void DestroyInternal()
    {

    }

    protected virtual void Reset(long newTriggerDate, long newId)
    {
        TriggerDate = newTriggerDate;
        SpawnDate = World.CurrentDate;

        PrevId = Id;
        Id = newId;

        FailedToTrigger = false;
    }

    public abstract void Reset(long newTriggerDate);
}

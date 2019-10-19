using UnityEngine;
using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(CellGroupEventGeneratorEvent))]
[ProtoInclude(200, typeof(ExpandPolityProminenceEvent))]
[ProtoInclude(300, typeof(MigrateGroupEvent))]
[ProtoInclude(400, typeof(TribeFormationEvent))]
[ProtoInclude(500, typeof(UpdateCellGroupEvent))]
public abstract class CellGroupEvent : WorldEvent
{
    [ProtoMember(1)]
    public long GroupId;

    public CellGroup Group;

    public CellGroupEvent()
    {

    }

    public CellGroupEvent(CellGroup group, long triggerDate, long eventTypeId, long? id = null, long originalSpawnDate = -1) :
        base(
        group.World, 
        triggerDate, 
        id ?? GenerateUniqueIdentifier(@group, triggerDate, eventTypeId), 
        eventTypeId, 
        originalSpawnDate)
    {
        Group = group;
        GroupId = Group.Id;

#if DEBUG
        GenerateDebugMessage(false);
#endif
    }

    public override WorldEventSnapshot GetSnapshot()
    {
        return new CellGroupEventSnapshot(this);
    }

    public static long GenerateUniqueIdentifier(CellGroup group, long triggerDate, long eventTypeId)
    {
#if DEBUG
        if (triggerDate >= World.MaxSupportedDate)
        {
            Debug.LogWarning("'triggerDate' shouldn't be greater than " + World.MaxSupportedDate + " (triggerDate = " + triggerDate + ")");
        }
#endif

        long id = (triggerDate * 1000000000L) + (group.Longitude * 1000000L) + (group.Latitude * 1000L) + eventTypeId;

        return id;
    }

#if DEBUG
    protected void GenerateDebugMessage(bool isReset)
    {
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            //			if (Group.Id == Manager.TracingData.GroupId) {
            string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

            SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("CellGroupEvent - Group:" + groupId + ", Type: " + this.GetType(),
                "SpawnDate: " + SpawnDate +
                ", TriggerDate: " + TriggerDate +
                //				", isReset: " + isReset + 
                "", SpawnDate);

            Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //			}
        }
    }
#endif

    public override bool IsStillValid()
    {
        if (!base.IsStillValid())
        {
            return false;
        }

        return Group != null && Group.StillPresent;
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        //if (GroupId == 0)
         //   return;

        Group = World.GetGroup(GroupId);

        if (Group == null)
        {
            throw new System.Exception("CellGroupEvent: Group with Id:" + GroupId + " not found");
        }
    }

    protected override void DestroyInternal()
    {
        base.DestroyInternal();
    }

    public virtual void Reset(long newTriggerDate)
    {
        Reset(newTriggerDate, GenerateUniqueIdentifier(Group, newTriggerDate, TypeId));

#if DEBUG
        GenerateDebugMessage(true);
#endif
    }
}

using ProtoBuf;

[ProtoContract]
public class UpdateCellGroupEvent : CellGroupEvent
{
    public UpdateCellGroupEvent()
    {
        DoNotSerialize = true;
    }

    public UpdateCellGroupEvent(CellGroup group, long triggerDate, long? id = null, long originalSpawnDate = -1) :
        base(group, triggerDate, UpdateCellGroupEventId, id, originalSpawnDate)
    {
        DoNotSerialize = true;
    }

    public override void Trigger()
    {
        World.AddGroupToUpdate(Group);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        Group.UpdateEvent = this;
    }
}

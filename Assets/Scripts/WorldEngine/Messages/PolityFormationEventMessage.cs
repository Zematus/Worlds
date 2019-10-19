using ProtoBuf;

[ProtoContract]
public class PolityFormationEventMessage : CellEventMessage {

	[ProtoMember(1)]
	public bool First = false;

	[ProtoMember(2)]
	public long PolityId;

	public PolityFormationEventMessage () {

	}

	public PolityFormationEventMessage (Polity polity, long date) : base (polity.CoreGroup.Cell, WorldEvent.PolityFormationEventId, date) {

		PolityId = polity.Id;
	}

    protected override string GenerateMessage()
    {
        PolityInfo polityInfo = World.GetPolityInfo(PolityId);

        if (First)
        {
            return "The first polity, " + polityInfo.Name.BoldText + ", formed at " + Position;
        }
        else
        {
            return "A new polity, " + polityInfo.Name.BoldText + ", formed at " + Position;
        }
    }
}

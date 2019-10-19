using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(AcceptedClanInlfuenceDemandEventMessage))]
[ProtoInclude(200, typeof(AcceptedFosterRelationshipAttemptEventMessage))]
[ProtoInclude(300, typeof(AcceptedMergeTribesOfferEventMessage))]
[ProtoInclude(400, typeof(AvoidFosterRelationshipEventMessage))]
[ProtoInclude(500, typeof(AvoidMergeTribesAttemptEventMessage))]
[ProtoInclude(600, typeof(AvoidOpeningTribeEventMessage))]
[ProtoInclude(700, typeof(OpenTribeEventMessage))]
[ProtoInclude(800, typeof(PreventTribeSplitEventMessage))]
[ProtoInclude(900, typeof(RejectedClanInlfuenceDemandEventMessage))]
[ProtoInclude(1000, typeof(RejectedFosterRelationshipAttemptEventMessage))]
[ProtoInclude(1100, typeof(RejectedMergeTribesOfferEventMessage))]
public abstract class PolityEventMessage : WorldEventMessage {

	[ProtoMember(1)]
	public long PolityId;

    public PolityInfo PolityInfo => World.GetPolityInfo(PolityId);

    public PolityEventMessage()
    {

    }

    public PolityEventMessage(Polity polity, long id, long date) : base(polity.World, id, date)
    {
        PolityId = polity.Id;
    }
}

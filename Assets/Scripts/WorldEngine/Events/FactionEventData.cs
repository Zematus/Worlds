using ProtoBuf;

[ProtoContract]
public class FactionEventData : WorldEventData {

    [ProtoMember(1)]
    public long OriginalPolityId;

	public FactionEventData () {

	}

	public FactionEventData (FactionEvent e) : base (e) {

		OriginalPolityId = e.OriginalPolityId;
	}
}

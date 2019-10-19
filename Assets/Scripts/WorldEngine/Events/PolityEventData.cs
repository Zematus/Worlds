using ProtoBuf;

[ProtoContract]
public class PolityEventData : WorldEventData {

    [ProtoMember(1)]
    public long OriginalDominantFactionId;

	public PolityEventData () {

	}

	public PolityEventData (PolityEvent e) : base (e) {

		OriginalDominantFactionId = e.OriginalDominantFactionId;
	}
}

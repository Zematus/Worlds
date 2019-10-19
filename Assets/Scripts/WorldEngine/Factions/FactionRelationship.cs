using ProtoBuf;

[ProtoContract]
public class FactionRelationship {

    [ProtoMember(1)]
    public long Id;

	[ProtoMember(2)]
	public float Value;

	public Faction Faction;

	public FactionRelationship () {
	}

	public FactionRelationship (Faction faction, float value) {

		Faction = faction;

		Id = faction.Id;

		Value = value;
	}
}

using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(CellGroup))]
[ProtoInclude(200, typeof(MigratingGroup))]
public abstract class HumanGroup : ISynchronizable {

    [ProtoMember(1)]
    public bool MigrationTagged;

	public World World;

	public HumanGroup () {
	}

	public HumanGroup (World world) {

		MigrationTagged = false;

		World = world;
	}

	public virtual void Synchronize () {
	}

	public virtual void FinalizeLoad () {

		if (MigrationTagged) {
		
			World.MigrationTagGroup (this);
		}
	}
}

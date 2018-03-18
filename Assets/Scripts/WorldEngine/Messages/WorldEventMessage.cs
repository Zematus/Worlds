using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class WorldEventMessage {

	public const long UpdateCellGroupEventId = 0;
	public const long MigrateGroupEventId = 1;
	public const long SailingDiscoveryEventId = 2;
	public const long TribalismDiscoveryEventId = 3;
	public const long TribeFormationEventId = 4;
	public const long BoatMakingDiscoveryEventId = 5;
	public const long PlantCultivationDiscoveryEventId = 6;

	public const long ClanSplitEventId = 7;
	public const long PreventClanSplitEventId = 8;

	public const long ExpandPolityInfluenceEventId = 20;

	public const long TribeSplitEventId = 21;
	public const long SplitingClanPreventTribeSplitEventId = 25;

	public const long PolityFormationEventId = 22;
	public const long ClanCoreMigrationEventId = 23;
	public const long FactionUpdateEventId = 24;

	[XmlAttribute]
	public long Id;

	[XmlAttribute]
	public long Date;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public string Message {
		get { return GenerateMessage (); }
	}

	public WorldEventMessage () {
	
	}

	public WorldEventMessage (World world, long id, long date) {
	
		World = world;
		Id = id;
		Date = date;
	}

	protected abstract string GenerateMessage ();
}

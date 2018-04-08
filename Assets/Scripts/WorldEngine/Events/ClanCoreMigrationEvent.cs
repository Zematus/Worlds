using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanCoreMigrationEvent : FactionEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 500;

	private CellGroup _targetGroup;

	public ClanCoreMigrationEvent () {

		DoNotSerialize = true;
	}

	public ClanCoreMigrationEvent (Clan clan, long originalTribeId, long triggerDate) : base (clan, originalTribeId, triggerDate, ClanCoreMigrationEventId) {

		DoNotSerialize = true;
	}

	public ClanCoreMigrationEvent (Clan clan, long triggerDate) : base (clan, triggerDate, ClanCoreMigrationEventId) {

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_CORE_MIGRATION_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		long targetDate = (long)(clan.World.CurrentDate + dateSpan) + CellGroup.GenerationSpan;

		return targetDate;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

//		#if DEBUG
//		if (Faction.Polity.Territory.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		Clan clan = Faction as Clan;

		_targetGroup = clan.GetCoreGroupMigrationTarget ();

		if (_targetGroup == null)
			return false;

		return Faction.ShouldMigrateFactionCore (Faction.CoreGroup, _targetGroup);
	}

	public override void Trigger () {

		World.AddGroupToUpdate (_targetGroup);

		Faction.SetToUpdate ();

		Faction.PrepareNewCoreGroup (_targetGroup);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {

			Clan clan = Faction as Clan;

			clan.CoreMigrationEvent = this;

			clan.CoreMigrationEventDate = CalculateTriggerDate (clan);

			Reset (clan.CoreMigrationEventDate);
			clan.CoreMigrationEventOriginalTribeId = OriginalPolityId;

			World.InsertEventToHappen (this);
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Clan clan = Faction as Clan;

		clan.CoreMigrationEvent = this;
	}
}

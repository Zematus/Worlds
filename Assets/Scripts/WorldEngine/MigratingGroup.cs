using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class MigratingGroup : HumanGroup {
	
	[XmlAttribute]
	public float PercentPopulation;

	[XmlAttribute]
	public int TargetCellLongitude;
	[XmlAttribute]
	public int TargetCellLatitude;

	[XmlAttribute]
	public int Population = 0;
	
	[XmlAttribute]
	public long SourceGroupId;

	//public CellCulture Culture;
	public BufferCulture Culture;

	[XmlIgnore]
	public List<Faction> FactionCoresToMigrate = new List<Faction> ();
	
	[XmlIgnore]
	public TerrainCell TargetCell;
	
	[XmlIgnore]
	public CellGroup SourceGroup;

	[XmlIgnore]
	public List <PolityInfluence> PolityInfluences;

	public MigratingGroup () {
	}

	public MigratingGroup (World world, float percentPopulation, CellGroup sourceGroup, TerrainCell targetCell) : base (world) {

		PercentPopulation = percentPopulation;

		#if DEBUG
		if (float.IsNaN (percentPopulation)) {

			Debug.Break ();
			throw new System.Exception ("float.IsNaN (percentPopulation)");
		}
		#endif

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (sourceGroup.Id == Manager.TracingData.GroupId) {
//				string groupId = "Id:" + sourceGroup.Id + "|Long:" + sourceGroup.Longitude + "|Lat:" + sourceGroup.Latitude;
//				string targetInfo = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"MigratingGroup:constructor - sourceGroup:" + groupId,
//					"CurrentDate: " + World.CurrentDate + 
//					", targetInfo: " + targetInfo + 
//					", percentPopulation: " + percentPopulation + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		TargetCell = targetCell;
		SourceGroup = sourceGroup;

		SourceGroupId = SourceGroup.Id;

		TargetCellLongitude = TargetCell.Longitude;
		TargetCellLatitude = TargetCell.Latitude;
	}
	
	public bool SplitFromSourceGroup () {
		
		if (SourceGroup == null)
			return false;
		
		if (!SourceGroup.StillPresent)
			return false;
		
		Population = SourceGroup.SplitGroup(this);
		
		if (Population <= 0)
			return false;
		
		//Culture = new CellCulture(SourceGroup, SourceGroup.Culture);
		Culture = new BufferCulture (SourceGroup.Culture);

		PolityInfluences = new List<PolityInfluence> ();

		foreach (PolityInfluence pi in SourceGroup.GetPolityInfluences ()) {

			PolityInfluences.Add (new PolityInfluence (pi.Polity, pi.Value));
		}

		TryMigrateFactionCores ();

		return true;
	}

	private void TryMigrateFactionCores () {

		int sourcePopulation = SourceGroup.Population;
		int targetPopulation = 0;
		int targetNewPopulation = Population;

		CellGroup targetGroup = TargetCell.Group;
		if (targetGroup != null) {
			targetPopulation = targetGroup.Population;
			targetNewPopulation += targetPopulation;
		}

		int sumPopulation = sourcePopulation + targetNewPopulation;

		foreach (Faction faction in SourceGroup.GetFactionCores ()) {

			float sourceGroupInfluence = SourceGroup.GetPolityInfluence (faction.Polity).Value;
			float targetGroupInfluence = 1f;

			if (targetGroup != null) {
				PolityInfluence piTarget = targetGroup.GetPolityInfluence (faction.Polity);
				if (piTarget != null)
					targetGroupInfluence = piTarget.Value;
			}

			float targetNewGroupInfluence = ((sourceGroupInfluence * Population) + (targetGroupInfluence * targetPopulation)) / targetNewPopulation;

			float migrateCoreFactor = ((sourceGroupInfluence * sourcePopulation) + (targetNewGroupInfluence * targetNewPopulation)) / sumPopulation;

			float randomValue = SourceGroup.GetNextLocalRandomFloat (RngOffsets.MIGRATING_GROUP_MOVE_FACTION_CORE + (int)faction.Id);

			if (randomValue >= migrateCoreFactor)
				FactionCoresToMigrate.Add (faction);
		}
	}

	public void MoveToCell () {
		
		if (Population <= 0)
			return;

		CellGroup targetGroup = TargetCell.Group;

		if (targetGroup != null) {

			if (targetGroup.StillPresent) {

				targetGroup.MergeGroup (this);

				if (SourceGroup.MigrationTagged) {
					World.MigrationTagGroup (TargetCell.Group);
				}
			}
		} else {

			targetGroup = new CellGroup (this, Population);

			World.AddGroup (targetGroup);
		
			if (SourceGroup.MigrationTagged) {
				World.MigrationTagGroup (targetGroup);
			}
		}

		foreach (Faction faction in FactionCoresToMigrate) {

			faction.SetCoreGroup (targetGroup);
		}
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();
		
		TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];
		
		SourceGroup = World.GetGroup (SourceGroupId);
	}
}

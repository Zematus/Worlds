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

	[XmlAttribute("MigDir")]
	public int MigrationDirectionInt;

	//public CellCulture Culture;
	public BufferCulture Culture;

	[XmlIgnore]
	public List<Faction> FactionCoresToMigrate = new List<Faction> ();
	
	[XmlIgnore]
	public TerrainCell TargetCell;
	
	[XmlIgnore]
	public CellGroup SourceGroup;

	[XmlIgnore]
	public List <PolityProminence> PolityProminences;

	[XmlIgnore]
	public Direction MigrationDirection;

	public MigratingGroup () {
	}

	public MigratingGroup (World world, float percentPopulation, CellGroup sourceGroup, TerrainCell targetCell, Direction migrationDirection) : base (world) {

		MigrationDirection = migrationDirection;

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

		PolityProminences = new List<PolityProminence> ();

		foreach (PolityProminence pi in SourceGroup.GetPolityProminences ()) {

			PolityProminences.Add (new PolityProminence (pi.Polity, pi.Value));
		}

		TryMigrateFactionCores ();

		return true;
	}

	private void TryMigrateFactionCores () {

		int targetPopulation = 0;
		int targetNewPopulation = Population;

		CellGroup targetGroup = TargetCell.Group;
		if (targetGroup != null) {
			targetPopulation = targetGroup.Population;
			targetNewPopulation += targetPopulation;
		}

		foreach (Faction faction in SourceGroup.GetFactionCores ()) {

			PolityProminence pi = SourceGroup.GetPolityProminence (faction.Polity);

			if (pi == null) {
				Debug.LogError ("Unable to find Polity with Id: " + faction.Polity.Id);
			}

			float sourceGroupProminence = pi.Value;
			float targetGroupProminence = sourceGroupProminence;

			if (targetGroup != null) {
				PolityProminence piTarget = targetGroup.GetPolityProminence (faction.Polity);

				if (piTarget != null)
					targetGroupProminence = piTarget.Value;
				else 
					targetGroupProminence = 0f;
			}

			float targetNewGroupProminence = ((sourceGroupProminence * Population) + (targetGroupProminence * targetPopulation)) / targetNewPopulation;

			if (faction.ShouldMigrateFactionCore (SourceGroup, TargetCell, targetNewGroupProminence, targetNewPopulation))
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

			faction.SetToUpdate ();

			faction.PrepareNewCoreGroup (targetGroup);
		}
	}

	public override void Synchronize ()
	{
		MigrationDirectionInt = (int)MigrationDirection;
	}
	
	public override void FinalizeLoad () {

		MigrationDirection = (Direction)MigrationDirectionInt;

		base.FinalizeLoad ();
		
		TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];
		
		SourceGroup = World.GetGroup (SourceGroupId);
	}
}

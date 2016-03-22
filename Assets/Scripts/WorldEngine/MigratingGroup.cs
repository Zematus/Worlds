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
	public int SourceGroupId;
	
	[XmlIgnore]
	public TerrainCell TargetCell;
	
	[XmlIgnore]
	public CellGroup SourceGroup;
	
	[XmlIgnore]
	public int Population = 0;

	[XmlIgnore]
	public CellCulture Culture;

	[XmlIgnore]
	public Dictionary <Polity, float> PolityInfluences;

	public MigratingGroup () {
	}

	public MigratingGroup (World world, float percentPopulation, CellGroup sourceGroup, TerrainCell targetCell) : base (world) {

		PercentPopulation = percentPopulation;

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
		
		Culture = new CellCulture(SourceGroup, SourceGroup.Culture);

		PolityInfluences = new Dictionary<Polity, float> (SourceGroup.PolityInfluences);

		return true;
	}

	public void MoveToCell () {
		
		if (Population <= 0)
			return;

		if (TargetCell.Group != null) {

			if (TargetCell.Group.StillPresent) {

				TargetCell.Group.MergeGroup(this);

				if (SourceGroup.MigrationTagged) {
					World.MigrationTagGroup (TargetCell.Group);
				}

				return;
			}
		}

		CellGroup newGroup = new CellGroup (this, Population, Culture);

		World.AddGroup (newGroup);
		
		if (SourceGroup.MigrationTagged) {
			World.MigrationTagGroup (newGroup);
		}
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();
		
		TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];
		
		SourceGroup = World.GetGroup (SourceGroupId);
	}
}

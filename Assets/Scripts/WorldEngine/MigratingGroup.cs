using UnityEngine;
using System.Collections;
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
	public int SplitPopulation = 0;
	[XmlIgnore]
	public CellCulture SplitCulture;

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
		
		SplitPopulation = SourceGroup.SplitGroup(this);
		
		if (SplitPopulation <= 0)
			return false;
		
		SplitCulture = SourceGroup.Culture;

		return true;
	}

	public void MoveToCell () {
		
		if (SplitPopulation <= 0)
			return;

		foreach (CellGroup group in TargetCell.Groups) {

			if (group.StillPresent) {

				group.MergeGroup(this, SplitPopulation, SplitCulture);

				if (SourceGroup.IsTagged) {
					World.TagGroup (group);
				}

				return;
			}
		}

		CellGroup newGroup = new CellGroup (this, SplitPopulation, SplitCulture);

		World.AddGroup (newGroup);
		
		if (SourceGroup.IsTagged) {
			World.TagGroup (newGroup);
		}
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();
		
		TargetCell = World.Terrain[TargetCellLongitude][TargetCellLatitude];
		
		SourceGroup = World.FindCellGroup (SourceGroupId);
	}
}

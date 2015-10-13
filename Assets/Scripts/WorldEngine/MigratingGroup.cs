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

	public void AddToCell () {

		if (SourceGroup == null)
			return;

		if (!SourceGroup.StillPresent)
			return;
		
		int splitPopulation = SourceGroup.SplitGroup(this);

		if (splitPopulation <= 0)
			return;

		Culture splitCulture = SourceGroup.Culture;

		foreach (CellGroup group in TargetCell.Groups) {

			if (group.StillPresent) {

				group.MergeGroup(this, splitPopulation, splitCulture);

				if (SourceGroup.IsTagged) {
					World.TagGroup (group);
				}

				return;
			}
		}

		CellGroup newGroup = new CellGroup (this, splitPopulation, splitCulture);

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

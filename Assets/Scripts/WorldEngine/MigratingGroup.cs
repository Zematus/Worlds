using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class MigratingGroup : HumanGroup {

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

	public MigratingGroup (World world, int population, CellGroup sourceGroup, TerrainCell targetCell) : base (world, population) {

		TargetCell = targetCell;
		SourceGroup = sourceGroup;

		TargetCellLongitude = TargetCell.Longitude;
		TargetCellLatitude = TargetCell.Latitude;
	}

	public void AddToCell () {

		if (SourceGroup == null)
			return;

		if (!SourceGroup.StillPresent)
			return;

		if (SourceGroup.Population < Population)
			Population = SourceGroup.Population;

		SourceGroup.Population -= Population;

		if (SourceGroup.Population == 0) {
		
			World.AddGroupToRemove (SourceGroup);
		}

		foreach (CellGroup group in TargetCell.Groups) {

			if (group.StillPresent) {

				group.MergeGroup(this);

				if (SourceGroup.IsTagged) {
					World.TagGroup (group);
				}

				return;
			}
		}

		CellGroup newGroup = new CellGroup (this);

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

using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class MigratingGroup : HumanGroup {

	public TerrainCell TargetCell;

	public MigratingGroup () {
	}

	public MigratingGroup (World world, int population, TerrainCell targetCell) : base (world, population) {

		TargetCell = targetCell;
	}

	public void AddToCell () {

		if (TargetCell.Groups.Count > 0) {
		
			TargetCell.Groups[0].MergeGroup(this);
			return;
		}

		World.AddGroup (new CellGroup (this));
	}
}

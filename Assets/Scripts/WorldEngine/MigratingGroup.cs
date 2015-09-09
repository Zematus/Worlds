using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class MigratingGroup : HumanGroup {

	public TerrainCell TargetCell;

	public CellGroup SourceGroup;

	public MigratingGroup () {
	}

	public MigratingGroup (World world, int population, CellGroup sourceGroup, TerrainCell targetCell) : base (world, population) {

		TargetCell = targetCell;
		SourceGroup = sourceGroup;
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
				return;
			}
		}

		World.AddGroup (new CellGroup (this));
	}
}

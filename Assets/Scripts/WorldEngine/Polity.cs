using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class Polity : Synchronizable {

	[XmlAttribute]
	public int Id;

	public int CoreGroupId;

	public List<int> InfluencedGroupIds;

	public Territory Territory = new Territory ();

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup CoreGroup;

	[XmlIgnore]
	public List<CellGroup> InfluencedGroups = new List<CellGroup> ();

	public Polity () {
	
	}

	public Polity (CellGroup group) {

		World = group.World;

		Id = World.GeneratePolityId ();

		SetCoreGroup (group);
	}

	public void SetCoreGroup (CellGroup group) {

		CoreGroup = group;

		CoreGroupId = group.Id;

		Territory.AddCell (group.Cell);
	}

	public virtual void Synchronize () {

		InfluencedGroupIds = new List<int> (InfluencedGroups.Count);

		foreach (CellGroup g in InfluencedGroups) {

			InfluencedGroupIds.Add (g.Id);
		}
	}

	public virtual void FinalizeLoad () {

		CoreGroup = World.GetCellGroup (CoreGroupId);

		if (CoreGroup == null) {
			throw new System.Exception ("Missing Group with Id " + CoreGroupId);
		}

		foreach (int id in InfluencedGroupIds) {

			CellGroup group = World.GetCellGroup (id);

			if (group == null) {
				throw new System.Exception ("Missing Group with Id " + id);
			}

			InfluencedGroups.Add (group);
		}
	}
}

public class Territory {

	public List<WorldPosition> CellPositions = new List<WorldPosition> ();

	[XmlIgnore]
	public World World;

	private HashSet<TerrainCell> _cells = new HashSet<TerrainCell> ();

	public Territory () {
	
	}

	public Territory (World world) {

		World = world;
	}

	public bool AddCell (TerrainCell cell) {

		if (!_cells.Add (cell))
			return false;

		CellPositions.Add (cell.Position);

		cell.AddEncompassingTerritory (this);

		return true;
	}

	public bool RemoveCell (TerrainCell cell) {

		if (!_cells.Remove (cell))
			return false;

		CellPositions.Remove (cell.Position);

		cell.RemoveEncompassingTerritory (this);

		return true;
	}

	public void FinalizeLoad () {

		foreach (WorldPosition position in CellPositions) {

			TerrainCell cell = World.GetCell (position);

			if (cell == null) {
				throw new System.Exception ("Cell missing at position " + position.Longitude + "," + position.Latitude);
			}
		
			_cells.Add (cell);

			cell.AddEncompassingTerritory (this);
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Territory : ISynchronizable {

	public List<WorldPosition> CellPositions;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public Polity Polity;

	private HashSet<TerrainCell> _cells = new HashSet<TerrainCell> ();

	public Territory () {

	}

	public Territory (Polity polity) {

		World = polity.World;
		Polity = polity;
	}

	public bool AddCell (TerrainCell cell) {

		if (!_cells.Add (cell))
			return false;

		cell.AddEncompassingTerritory (this);

		Region cellRegion = cell.Region;

		if (cellRegion == null) {

			cellRegion = Region.TryGenerateRegion (cell);
			cellRegion.GenerateName (Polity);

			if (cellRegion != null) {
				World.AddRegion (cellRegion);
			}
		}

		return true;
	}

	public bool RemoveCell (TerrainCell cell) {

		if (!_cells.Remove (cell))
			return false;

		cell.RemoveEncompassingTerritory (this);

		return true;
	}

	public void Synchronize () {

		CellPositions = new List<WorldPosition> (_cells.Count);

		foreach (TerrainCell cell in _cells) {

			CellPositions.Add (cell.Position);
		}
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

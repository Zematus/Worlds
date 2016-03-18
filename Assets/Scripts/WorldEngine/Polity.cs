using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class Polity {

	public WorldPosition CoreCellPosition;

	public List<WorldPosition> TerritoryPositions = new List<WorldPosition> ();

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public TerrainCell CoreCell;

	private HashSet<TerrainCell> _territory = new HashSet<TerrainCell> ();

	public Polity () {
	
	}

	public Polity (TerrainCell coreCell) {

		World = coreCell.World;

		SetCoreCell (coreCell);

		AddCellToTerritory (coreCell);
	}

	public void SetCoreCell (TerrainCell cell) {

		CoreCell = cell;

		CoreCellPosition = cell.Position;
	}

	public bool AddCellToTerritory (TerrainCell cell) {

		if (!_territory.Add (cell))
			return false;

		TerritoryPositions.Add (cell.Position);

		return true;
	}

	public bool RemoveCellToTerritory (TerrainCell cell) {

		if (!_territory.Remove (cell))
			return false;

		TerritoryPositions.Remove (cell.Position);

		return true;
	}

	public void FinalizeLoad () {

		CoreCell = World.GetCell (CoreCellPosition);

		if (CoreCell == null) {

			throw new System.Exception ("Terrain cell missing at position " + CoreCellPosition.Longitude + "," + CoreCellPosition.Latitude);
		}

		foreach (WorldPosition position in TerritoryPositions) {

			TerrainCell cell = World.GetCell (position);

			if (cell == null) {
			
				throw new System.Exception ("Terrain cell missing at position " + position.Longitude + "," + position.Latitude);
			}
		
			_territory.Add (cell);
		}
	}
}

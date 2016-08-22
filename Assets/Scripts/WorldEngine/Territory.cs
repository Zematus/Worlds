using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Territory : ISynchronizable {

	public List<WorldPosition> CellPositions;

	[XmlIgnore]
	public bool IsSelected = false;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public Polity Polity;

	private HashSet<TerrainCell> _cells = new HashSet<TerrainCell> ();

	private HashSet<TerrainCell> _borderCells = new HashSet<TerrainCell> ();

	public Territory () {

	}

	public Territory (Polity polity) {

		World = polity.World;
		Polity = polity;
	}

	public ICollection<TerrainCell> GetCells () {

		return _cells;
	}

	private bool IsPartOfBorderInternal (TerrainCell cell) {

		if (!_cells.Contains (cell)) {
		
			return false;
		}

		foreach (TerrainCell nCell in cell.Neighbors.Values) {
		
			if (!_cells.Contains (nCell))
				return true;
		}

		return false;
	}

	public bool IsPartOfBorder (TerrainCell cell) {
	
		return _borderCells.Contains (cell);
	}

	public bool AddCell (TerrainCell cell) {

		if (!_cells.Add (cell))
			return false;

		cell.EncompassingTerritory = this;
		Manager.AddUpdatedCell (cell, CellUpdateType.Territory);

		if (IsPartOfBorderInternal (cell)) {
		
			_borderCells.Add (cell);

			foreach (TerrainCell nCell in cell.Neighbors.Values) {
			
				if (_borderCells.Contains (nCell)) {
				
					if (!IsPartOfBorderInternal (nCell)) {
						_borderCells.Remove (nCell);
						Manager.AddUpdatedCell (nCell, CellUpdateType.Territory);
					}
				}
			}
		}

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

		cell.EncompassingTerritory = null;
		Manager.AddUpdatedCell (cell, CellUpdateType.Territory);

		if (_borderCells.Contains (cell)) {

			_borderCells.Remove (cell);

			foreach (TerrainCell nCell in cell.Neighbors.Values) {

				if (IsPartOfBorderInternal (nCell)) {

					_borderCells.Add (nCell);
					Manager.AddUpdatedCell (nCell, CellUpdateType.Territory);
				}
			}
		}

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

			cell.EncompassingTerritory = this;
		}

		foreach (TerrainCell cell in _cells) {

			foreach (TerrainCell nCell in cell.Neighbors.Values) {

				if (!_cells.Contains (nCell)) {
					_borderCells.Add (cell);
					break;
				}
			}
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Route {

	public List<WorldPosition> CellPositions = new List<WorldPosition> ();

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public TerrainCell FirstCell;

	[XmlIgnore]
	public TerrainCell LastCell;

	[XmlIgnore]
	public List<TerrainCell> Cells = new List<TerrainCell> ();

	private bool _isTraversingSea = false;
	private Direction _traverseDirection;

	private float _currentCoastPreference = 4;
	private float _currentEndRoutePreference = 0;

	public Route () {
	
	}

	public Route (TerrainCell startCell) {

		World = startCell.World;

		FirstCell = startCell;
	
		AddCell (startCell);

		TerrainCell nextCell = startCell;

		while (true) {
		
			nextCell = ChooseNextSeaCell (nextCell);

			if (nextCell == null)
				break;

			AddCell (nextCell);

			if (nextCell.GetBiomePresence (Biome.Ocean) <= 0)
				break;
		}

		LastCell = nextCell;
	}

	public void AddCell (TerrainCell cell) {
	
		Cells.Add (cell);
		CellPositions.Add (cell.Position);
	}
		
	public TerrainCell ChooseNextSeaCell (TerrainCell currentCell) {

		if (_isTraversingSea)
			return ChooseNextDepthSeaCell (currentCell);
		else
			return ChooseNextCoastalCell (currentCell);
	}

	public TerrainCell ChooseNextDepthSeaCell (TerrainCell currentCell) {

		TerrainCell nextCell = currentCell.GetNeighborCell (_traverseDirection);

		if (nextCell == null)
			return null;

		if (nextCell.IsPartOfCoastline) {
			
			_currentCoastPreference += 4;
			_currentEndRoutePreference += 0.1f;

			_isTraversingSea = false;
		}

		return nextCell;
	}

	public TerrainCell ChooseNextCoastalCell (TerrainCell currentCell) {

		float totalWeight = 0;

		Dictionary<KeyValuePair<Direction, TerrainCell>, float> weights = new Dictionary<KeyValuePair<Direction, TerrainCell>, float> ();

		foreach (KeyValuePair<Direction, TerrainCell> nPair in currentCell.Neighbors) {

			TerrainCell nCell = nPair.Value;

			if (Cells.Contains (nCell))
				continue;

			float oceanPresence = nCell.GetBiomePresence (Biome.Ocean);

			float weight = oceanPresence;

			if (nCell.IsPartOfCoastline)
				weight *= _currentCoastPreference;

			weight += (1f - oceanPresence) * _currentEndRoutePreference;

			weights.Add (nPair, weight);

			totalWeight += weight;
		}

		KeyValuePair<Direction, TerrainCell> targetPair = CollectionUtility.WeightedSelection (weights, totalWeight, currentCell.GetNextLocalRandomFloat);

		if (!targetPair.Value.IsPartOfCoastline) {
		
			_isTraversingSea = true;
			_traverseDirection = targetPair.Key;
		}

		_currentEndRoutePreference += 0.1f;

		return targetPair.Value;
	}

	public bool ContainsCell (TerrainCell cell) {
	
		return Cells.Contains (cell);
	}

	public void FinalizeLoad () {

		TerrainCell currentCell = null;

		bool first = true;
	
		foreach (WorldPosition p in CellPositions) {

			currentCell = World.GetCell (p);

			if (currentCell == null) {
				throw new System.Exception ("Unable to find terrain cell at [" + currentCell.Longitude + "," + currentCell.Latitude + "]");
			}

			if (first) {

				FirstCell = currentCell;
				first = false;
			}

			Cells.Add (currentCell);
		}

		LastCell = currentCell;
	}
}

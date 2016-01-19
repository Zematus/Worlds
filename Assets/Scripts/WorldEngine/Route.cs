using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public struct WorldPosition {

	[XmlAttribute]
	public int Longitude;
	[XmlAttribute]
	public int Latitude;

	public WorldPosition (int longitude, int latitude) {
	
		Longitude = longitude;
		Latitude = latitude;
	}
}

public class Route {

	[XmlIgnore]
	public World World;

	public List<WorldPosition> CellPositions = new List<WorldPosition> ();

	private List<TerrainCell> _cells = new List<TerrainCell> ();

	private bool _isTraversingSea = false;
	private Direction _traverseDirection;

	private float _currentCoastPreference = 4;
	private float _currentEndRoutePreference = 0;

	public static Route BuildSeaRoute (TerrainCell startCell) {

		return null;
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

		if (nextCell.GetBiomePresence (Biome.Ocean) > 0)
			return nextCell;

		_currentCoastPreference += 4;
		_currentEndRoutePreference += 0.1f;

		return ChooseNextCoastalCell (currentCell);
	}

	public TerrainCell ChooseNextCoastalCell (TerrainCell currentCell) {

		float totalWeight = 0;

		Dictionary<TerrainCell, float> weights = new Dictionary<TerrainCell, float> ();

		weights.Add (currentCell, _currentEndRoutePreference);

		totalWeight += _currentEndRoutePreference;

		foreach (TerrainCell nCell in currentCell.Neighbors.Values) {

			if (_cells.Contains (nCell))
				continue;

			float weight = nCell.GetBiomePresence (Biome.Ocean);

			if (nCell.IsPartOfCoastline)
				weight *= _currentCoastPreference;

			weights.Add (nCell, weight);

			totalWeight += weight;
		}

		TerrainCell targetCell = CollectionUtility.WeightedSelection (weights, totalWeight, currentCell.GetNextLocalRandomFloat);

		_currentEndRoutePreference += 0.1f;

		return targetCell;
	}

	public bool ContainsCell (TerrainCell cell) {
	
		return _cells.Contains (cell);
	}
}

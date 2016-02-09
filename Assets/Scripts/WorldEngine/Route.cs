using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Route {

	public List<WorldPosition> CellPositions = new List<WorldPosition> ();

	[XmlAttribute]
	public float Length = 0;

	[XmlAttribute]
	public bool Consolidated = false;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public TerrainCell FirstCell;

	[XmlIgnore]
	public TerrainCell LastCell;

	[XmlIgnore]
	public List<TerrainCell> Cells = new List<TerrainCell> ();

	private const float CoastPreferenceIncrement = 400;

	private bool _isTraversingSea = false;
	private Direction _traverseDirection;
	private float _currentDirectionOffset = 0;

	private float _currentCoastPreference = CoastPreferenceIncrement;
	private float _currentEndRoutePreference = 0;

	public Route () {
	
	}

	public Route (TerrainCell startCell) {

		World = startCell.World;

		FirstCell = startCell;
	
		AddCell (startCell);

		TerrainCell nextCell = startCell;
		Direction nextDirection;

		while (true) {

			nextCell = ChooseNextSeaCell (nextCell, out nextDirection);

			if (nextCell == null)
				break;

			Length += CalculateDistance (nextCell, nextDirection); 

			AddCell (nextCell);

			if (nextCell.GetBiomePresence (Biome.Ocean) <= 0)
				break;
		}

		LastCell = nextCell;
	}

	public void Destroy () {

		if (!Consolidated)
			return;
	
		foreach (TerrainCell cell in Cells) {
		
			cell.RemoveCrossingRoute (this);
			Manager.AddUpdatedCell (cell);
		}
	}

	public void Consolidate () {

		if (Consolidated)
			return;

		foreach (TerrainCell cell in Cells) {

			cell.AddCrossingRoute (this);
			Manager.AddUpdatedCell (cell);
		}

		Consolidated = true;
	}

	public float CalculateDistance (TerrainCell cell, Direction direction) {

		float distanceFactor = TerrainCell.MaxWidth;

		if ((direction == Direction.Northeast) ||
		    (direction == Direction.Northwest) ||
		    (direction == Direction.Southeast) ||
		    (direction == Direction.Southwest)) {
		
			distanceFactor = Mathf.Sqrt (TerrainCell.MaxWidth + cell.Width);

		} else if ((direction == Direction.East) ||
		           (direction == Direction.West)) {

			distanceFactor = cell.Width;
		}

		return distanceFactor;
	}

	public void AddCell (TerrainCell cell) {
	
		Cells.Add (cell);
		CellPositions.Add (cell.Position);
	}
		
	public TerrainCell ChooseNextSeaCell (TerrainCell currentCell, out Direction direction) {

		if (_isTraversingSea)
			return ChooseNextDepthSeaCell (currentCell, out direction);
		else
			return ChooseNextCoastalCell (currentCell, out direction);
	}

	public TerrainCell ChooseNextDepthSeaCell (TerrainCell currentCell, out Direction direction) {

		Direction newDirection = _traverseDirection;
		float newOffset = _currentDirectionOffset;

		float deviation = 2 * FirstCell.GetNextLocalRandomFloat () - 1;
		deviation = (deviation * deviation + 1f) / 2f;
		deviation = newOffset - deviation;

//		LatitudeDirectionModifier (currentCell, out newDirection, out newOffset);

		if (deviation >= 0.5f) {
			newDirection = (Direction)(((int)_traverseDirection + 1) % 8);
		} else if (deviation < -0.5f) {
			newDirection = (Direction)(((int)_traverseDirection + 6) % 8);
		} else if (deviation < 0) {
			newDirection = (Direction)(((int)_traverseDirection + 7) % 8);
		}

		TerrainCell nextCell = currentCell.GetNeighborCell (newDirection);
		direction = newDirection;

		if (nextCell == null)
			return null;

		if (Cells.Contains (nextCell))
			return null;

		if (nextCell.IsPartOfCoastline) {
			
			_currentCoastPreference += CoastPreferenceIncrement;
			_currentEndRoutePreference += 0.1f;

			_isTraversingSea = false;
		}

		return nextCell;
	}

	public TerrainCell ChooseNextCoastalCell (TerrainCell currentCell, out Direction direction) {

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

		KeyValuePair<Direction, TerrainCell> targetPair = CollectionUtility.WeightedSelection (weights, totalWeight, FirstCell.GetNextLocalRandomFloat);

		TerrainCell targetCell = targetPair.Value;
		direction = targetPair.Key;

		if (targetCell == null) {

			throw new System.Exception ("targetCell is null");
		}

		if (!targetCell.IsPartOfCoastline) {
		
			_isTraversingSea = true;
			_traverseDirection = direction;

			_currentDirectionOffset = FirstCell.GetNextLocalRandomFloat();
		}

		_currentEndRoutePreference += 0.1f;

		return targetCell;
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

		if (Consolidated) {
		
			foreach (TerrainCell cell in Cells) {
				cell.AddCrossingRoute (this);
			}
		}

		LastCell = currentCell;
	}

	//	private void LatitudeDirectionModifier (TerrainCell currentCell, out Direction newDirection, out float newOffset) {
	//
	//		float modOffset;
	//
	//		switch (_traverseDirection) {
	//		case Direction.West:
	//			modOffset = _currentDirectionOffset / 2f;
	//			break;
	//		case Direction.Northwest:
	//			modOffset = 0.5f + _currentDirectionOffset / 2f;
	//			break;
	//		case Direction.North:
	//			modOffset = 0.5f + (1f - _currentDirectionOffset) / 2f;
	//			break;
	//		case Direction.Northeast:
	//			modOffset = (1f - _currentDirectionOffset) / 2f;
	//			break;
	//		case Direction.East:
	//			modOffset = _currentDirectionOffset / 2f;
	//			break;
	//		case Direction.Southeast:
	//			modOffset = 0.5f + _currentDirectionOffset / 2f;
	//			break;
	//		case Direction.South:
	//			modOffset = 0.5f + (1f - _currentDirectionOffset) / 2f;
	//			break;
	//		case Direction.Southwest:
	//			modOffset = (1f - _currentDirectionOffset) / 2f;
	//			break;
	//		default:
	//			throw new System.Exception ("Unhandled direction: " + _traverseDirection);
	//		}
	//
	//		float latFactor = Mathf.Sin(Mathf.PI * currentCell.Latitude / (float)World.Height);
	//
	//		modOffset *= latFactor;
	//
	//		switch (_traverseDirection) {
	//
	//		case Direction.West:
	//			newOffset = modOffset * 2f;
	//			newDirection = Direction.West;
	//			break;
	//
	//		case Direction.Northwest:
	//			newOffset = modOffset * 2f;
	//
	//			if (newOffset > 1) {
	//				newOffset -= 1f;
	//				newDirection = Direction.Northwest;
	//			} else {
	//				newDirection = Direction.West;
	//			}
	//
	//			break;
	//
	//		case Direction.North:
	//			newOffset = (1f - modOffset) * 2f;
	//
	//			if (newOffset > 1) {
	//				newOffset -= 1f;
	//				newDirection = Direction.Northeast;
	//			} else {
	//				newDirection = Direction.North;
	//			}
	//
	//			break;
	//
	//		case Direction.Northeast:
	//			newOffset = (1f - modOffset) * 2f;
	//			newDirection = Direction.Northeast;
	//			break;
	//
	//		case Direction.East:
	//			newOffset = modOffset * 2f;
	//			newDirection = Direction.East;
	//			break;
	//
	//		case Direction.Southeast:
	//			newOffset = modOffset * 2f;
	//
	//			if (newOffset > 1) {
	//				newOffset -= 1f;
	//				newDirection = Direction.Southeast;
	//			} else {
	//				newDirection = Direction.East;
	//			}
	//
	//			break;
	//
	//		case Direction.South:
	//			newOffset = (1f - modOffset) * 2f;
	//
	//			if (newOffset > 1) {
	//				newOffset -= 1f;
	//				newDirection = Direction.Southwest;
	//			} else {
	//				newDirection = Direction.South;
	//			}
	//
	//			break;
	//
	//		case Direction.Southwest:
	//			newOffset = (1f - modOffset) * 2f;
	//			newDirection = Direction.Southwest;
	//			break;
	//
	//		default:
	//			throw new System.Exception ("Unhandled direction: " + _traverseDirection);
	//		}
	//	}
}

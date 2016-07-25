using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class Region : Synchronizable {

	public const int MinRegionSize = 0;
	public const float MinBiomePresence = 0.2f;
	public const float MinBiomePresenceStart = 0.2f;
	public const float BaseMaxAltitudeDifference = 2000;
	public const int AltitudeRoundnessTarget = 1000;

//	public const float AltitudeDifferenceDistanceFactor = 0.01f;

	public const float MinClosedness = 0.8f;

	public long Id;

	public List<WorldPosition> CellPositions;

	[XmlIgnore]
	public World World;

	private HashSet<TerrainCell> _cells = new HashSet<TerrainCell> ();

	public Region () {

	}

	public Region (World world) {

		World = world;

		Id = World.GenerateRegionId ();
	}

	public bool AddCell (TerrainCell cell) {

		if (!_cells.Add (cell))
			return false;

		cell.Region = this;

		return true;
	}

	public bool RemoveCell (TerrainCell cell) {

		if (!_cells.Remove (cell))
			return false;

		cell.Region = null;

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

			cell.Region = this;
		}
	}

	public static Region TryGenerateRegion (TerrainCell startCell) {

		if (startCell.Region != null)
			return null;

		if (startCell.MostBiomePresence < MinBiomePresenceStart)
			return null;

		Region region = TryGenerateBiomeRegion (startCell, startCell.BiomeWithMostPresence);

		return region;
	}

	public static Region TryGenerateBiomeRegion (TerrainCell startCell, string biomeName) {

		int regionSize = 1;

		// round the base altitude
		float baseAltitude = AltitudeRoundnessTarget * Mathf.Round (startCell.Altitude / AltitudeRoundnessTarget);
//		float baseAltitude = startCell.Altitude;

		HashSet<TerrainCell> acceptedCells = new HashSet<TerrainCell> ();
		HashSet<TerrainCell> unacceptedCells = new HashSet<TerrainCell> ();

		acceptedCells.Add (startCell);

		HashSet<TerrainCell> cellsToExplore = new HashSet<TerrainCell> ();

		foreach (TerrainCell cell in startCell.Neighbors.Values) {

			cellsToExplore.Add (cell);
		}

		bool addedAcceptedCells = true;

		int borderCells = 0;

		while (addedAcceptedCells) {
			HashSet<TerrainCell> nextCellsToExplore = new HashSet<TerrainCell> ();
			addedAcceptedCells = false;

			if (cellsToExplore.Count <= 0)
				break;

			float closedness = cellsToExplore.Count / (float)(cellsToExplore.Count + borderCells);

			foreach (TerrainCell cell in cellsToExplore) {

//				float meanWidth = (startCell.Width + cell.Width) / 2f;
//				int longDifference = Mathf.Abs (startCell.Position.Longitude - cell.Position.Longitude);
//				float distanceLong = Mathf.Min(cell.World.Width - longDifference, longDifference) * meanWidth;
//				float distanceLat = Mathf.Abs (startCell.Latitude - cell.Latitude) * cell.Height;
//				float distance = Mathf.Sqrt(Mathf.Pow(distanceLat, 2) + Mathf.Pow(distanceLong, 2));
//
//				float maxAltitudeDifference = BaseMaxAltitudeDifference / (1 + AltitudeDifferenceDistanceFactor * distance);

				float closednessFactor = closedness / MinClosedness - MinClosedness * 0.2f;

				float maxAltitudeDifference = BaseMaxAltitudeDifference * closednessFactor;

				bool accepted = false;

				if ((cell.Region == null) &&
				    (cell.BiomeWithMostPresence == biomeName) &&
				    (cell.MostBiomePresence >= MinBiomePresence)) {

//					if ((closedness > MinClosedness) ||
//						(Mathf.Abs (cell.Altitude - baseAltitude) < maxAltitudeDifference)) {
//					if (closedness > MinClosedness) {
					if (Mathf.Abs (cell.Altitude - baseAltitude) < maxAltitudeDifference) {
				
						accepted = true;
						acceptedCells.Add (cell);
						addedAcceptedCells = true;
						regionSize++;

						foreach (TerrainCell ncell in cell.Neighbors.Values) {

							if (cellsToExplore.Contains (ncell))
								continue;

							if (unacceptedCells.Contains (ncell))
								continue;

							if (acceptedCells.Contains (ncell))
								continue;

							nextCellsToExplore.Add (ncell);
						}
					}
				}

				if (!accepted) {
					unacceptedCells.Add (cell);
					borderCells++;
				}
			}

			cellsToExplore = nextCellsToExplore;
		}

		if (regionSize < MinRegionSize)
			return null;

		Region region = new Region (startCell.World);

		foreach (TerrainCell cell in acceptedCells) {
			
			region.AddCell (cell);
		}

		return region;
	}
}

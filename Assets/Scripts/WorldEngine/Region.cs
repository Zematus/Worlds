using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public enum RegionAttribute {
	Glacier,
	IceCap,
	Ocean,
	Grassland,
	Forest,
	Taiga,
	Tundra,
	Desert,
	Rainforest,
	Jungle,
	Valley,
	Highlands,
	Range,
	Hills,
	Mountains,
	Basin,
	Plain,
	Delta,
	Peninsula,
	Island,
	Archipelago,
	Chanel,
	Gulf,
	Sound,
	Lake,
	Sea,
	Continent,
	Strait
}

public abstract class Region : Synchronizable {

	public const float BaseMaxAltitudeDifference = 1000;
	public const int AltitudeRoundnessTarget = 2000;

	public const float MaxClosedness = 0.5f;

	public List<RegionAttribute> Attributes = new List<RegionAttribute>();

	[XmlAttribute]
	public long Id;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public float AverageAltitude = 0;
	[XmlIgnore]
	public float AverageRainfall = 0;
	[XmlIgnore]
	public float AverageTemperature = 0;

	[XmlIgnore]
	public float AverageSurvivability = 0;
	[XmlIgnore]
	public float AverageForagingCapacity = 0;
	[XmlIgnore]
	public float AverageAccessibility = 0;
	[XmlIgnore]
	public float AverageArability = 0;

	[XmlIgnore]
	public float AverageFarmlandPercentage = 0;

	[XmlIgnore]
	public float TotalArea = 0;

	[XmlIgnore]
	public string BiomeWithMostPresence = null;
	[XmlIgnore]
	public float MostBiomePresence = 0;

	[XmlIgnore]
	public List<string> PresentBiomeNames = new List<string>();
	[XmlIgnore]
	public List<float> BiomePresences = new List<float>();

	[XmlIgnore]
	public float AverageBorderAltitude = 0;
	[XmlIgnore]
	public float MinAltitude = float.MaxValue;
	[XmlIgnore]
	public float MaxAltitude = float.MinValue;

	[XmlIgnore]
	public HashSet<TerrainCell> BorderCells = new HashSet<TerrainCell> ();

	protected Dictionary<string, float> _biomePresences;

	public Region () {

	}

	public Region (World world) {

		World = world;

		Id = World.GenerateRegionId ();
	}

	public Region (World world, int id) {

		World = world;

		Id = id;
	}

	public bool IsBorderCell (TerrainCell cell) {

		return BorderCells.Contains (cell);
	}

	public abstract void Synchronize ();

	public abstract void FinalizeLoad ();

	public static Region TryGenerateRegion (TerrainCell startCell) {

		if (startCell.Region != null)
			return null;

		Region region = TryGenerateBiomeRegion (startCell, startCell.BiomeWithMostPresence);

		return region;
	}

	public static Region TryGenerateBiomeRegion (TerrainCell startCell, string biomeName) {

		int regionSize = 1;

		// round the base altitude
		float baseAltitude = AltitudeRoundnessTarget * Mathf.Round (startCell.Altitude / AltitudeRoundnessTarget);

		HashSet<TerrainCell> acceptedCells = new HashSet<TerrainCell> ();
		HashSet<TerrainCell> unacceptedCells = new HashSet<TerrainCell> ();

		acceptedCells.Add (startCell);

		HashSet<TerrainCell> cellsToExplore = new HashSet<TerrainCell> ();

		foreach (TerrainCell cell in startCell.Neighbors.Values) {

			cellsToExplore.Add (cell);
		}

		bool addedAcceptedCells = true;

		int borderCells = 0;

		float maxClosedness = 0;

		while (addedAcceptedCells) {
			HashSet<TerrainCell> nextCellsToExplore = new HashSet<TerrainCell> ();
			addedAcceptedCells = false;

			if (cellsToExplore.Count <= 0)
				break;

			float closedness = 1 - cellsToExplore.Count / (float)(cellsToExplore.Count + borderCells);

			if (closedness > maxClosedness)
				maxClosedness = closedness;

			foreach (TerrainCell cell in cellsToExplore) {

				float closednessFactor = 1;
				float cutOffFactor = 2;

				if (MaxClosedness < 1) {
					closednessFactor = (1 + MaxClosedness / cutOffFactor) * (1 - closedness) / (1 - MaxClosedness) - MaxClosedness / cutOffFactor;
				}

				float maxAltitudeDifference = BaseMaxAltitudeDifference * closednessFactor;

				bool accepted = false;

				string cellBiomeName = cell.BiomeWithMostPresence;

				if ((cell.Region == null) && (cellBiomeName == biomeName)) {

					if (Mathf.Abs (cell.Altitude - baseAltitude) < maxAltitudeDifference) {

						accepted = true;
						acceptedCells.Add (cell);
						addedAcceptedCells = true;
						regionSize++;

						foreach (KeyValuePair<Direction, TerrainCell> pair in cell.Neighbors) {

							TerrainCell ncell = pair.Value;

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

		CellRegion region = new CellRegion (startCell.World);

		foreach (TerrainCell cell in acceptedCells) {

			region.AddCell (cell);
		}

		region.EvaluateAttributes ();

		return region;
	}

	public static Region TrySplitRegion (Region region) {

		return null;
	}
}

//public class SuperRegion : Region {
//
//	public SuperRegion () {
//
//	}
//
//	public SuperRegion (World world, int id) : base (world, id) {
//
//	}
//}

public class CellRegion : Region {

	public List<WorldPosition> CellPositions;

	private HashSet<TerrainCell> _cells = new HashSet<TerrainCell> ();

	public CellRegion () {

	}

	public CellRegion (World world) : base (world) {
		
	}

	public bool AddCell (TerrainCell cell) {

		if (!_cells.Add (cell))
			return false;

		cell.Region = this;
		Manager.AddUpdatedCell (cell, CellUpdateType.Region);

		return true;
	}

	public static bool ValidateIfBorderCell (Region region, TerrainCell cell) {

		foreach (TerrainCell nCell in cell.Neighbors.Values) {

			if (nCell.Region != region)
				return true;
		}

		return false;
	}

	public void EvaluateAttributes () {

		Dictionary<string, float> biomePresences = new Dictionary<string, float> ();

		foreach (TerrainCell cell in _cells) {

			if (ValidateIfBorderCell (this, cell)) {
			
				BorderCells.Add (cell);
			}

			if (MinAltitude > cell.Altitude) {
				MinAltitude = cell.Altitude;
			}

			if (MaxAltitude < cell.Altitude) {
				MaxAltitude = cell.Altitude;
			}
			
			float cellArea = cell.Area;

			AverageAltitude += cell.Altitude * cellArea;
			AverageRainfall += cell.Rainfall * cellArea;
			AverageTemperature += cell.Temperature * cellArea;

			AverageSurvivability += cell.Survivability * cellArea;
			AverageForagingCapacity += cell.ForagingCapacity * cellArea;
			AverageAccessibility += cell.Accessibility * cellArea;
			AverageArability += cell.Arability * cellArea;

			AverageFarmlandPercentage += cell.FarmlandPercentage * cellArea;

			foreach (string biomeName in cell.PresentBiomeNames) {

				float presence = cell.GetBiomePresence(biomeName) * cellArea;

				if (biomePresences.ContainsKey (biomeName)) {
					biomePresences [biomeName] += presence;
				} else {
					biomePresences.Add (biomeName, presence);
				}
			}

			TotalArea += cellArea;
		}

		AverageAltitude /= TotalArea;
		AverageRainfall /= TotalArea;
		AverageTemperature /= TotalArea;

		AverageSurvivability /= TotalArea;
		AverageForagingCapacity /= TotalArea;
		AverageAccessibility /= TotalArea;
		AverageArability /= TotalArea;

		AverageFarmlandPercentage /= TotalArea;

		PresentBiomeNames = new List<string> (biomePresences.Count);
		BiomePresences = new List<float> (biomePresences.Count);

		_biomePresences = new Dictionary<string, float> (biomePresences.Count);

		foreach (KeyValuePair<string, float> pair in biomePresences) {

			float presence = pair.Value / TotalArea;

			PresentBiomeNames.Add (pair.Key);
			BiomePresences.Add (presence);

			_biomePresences.Add (pair.Key, presence);

			if (MostBiomePresence < presence) {
			
				MostBiomePresence = presence;
				BiomeWithMostPresence = pair.Key;
			}
		}

		EvaluateBorderAttributes ();

		DefineAttributes ();
	}

	public void EvaluateBorderAttributes () {

		float totalBorderArea = 0;

		foreach (TerrainCell cell in BorderCells) {

			float cellArea = cell.Area;

			AverageBorderAltitude += cell.Altitude * cellArea;

			totalBorderArea += cellArea;
		}

		AverageBorderAltitude /= totalBorderArea;
	}

	public bool RemoveCell (TerrainCell cell) {

		if (!_cells.Remove (cell))
			return false;

		cell.Region = null;
		Manager.AddUpdatedCell (cell, CellUpdateType.Region);

		return true;
	}

	public override void Synchronize () {

		CellPositions = new List<WorldPosition> (_cells.Count);

		foreach (TerrainCell cell in _cells) {

			CellPositions.Add (cell.Position);
		}
	}

	public override void FinalizeLoad () {

		foreach (WorldPosition position in CellPositions) {

			TerrainCell cell = World.GetCell (position);

			if (cell == null) {
				throw new System.Exception ("Cell missing at position " + position.Longitude + "," + position.Latitude);
			}

			_cells.Add (cell);

			cell.Region = this;
		}

		EvaluateAttributes ();
	}

	private void DefineAttributes () {

		if (AverageAltitude > (AverageBorderAltitude + 100f)) {

			Attributes.Add (RegionAttribute.Highlands);
		}

		if (AverageAltitude < (AverageBorderAltitude - 100f)) {

			Attributes.Add (RegionAttribute.Valley);

			if (AverageRainfall > 1000) {

				Attributes.Add (RegionAttribute.Basin);
			}
		}

		if (MostBiomePresence > 0.65f) {
		
			switch (BiomeWithMostPresence) {

			case "Desert":
				Attributes.Add (RegionAttribute.Desert);
				break;

			case "Desertic Tundra":
				Attributes.Add (RegionAttribute.Desert);
				break;

			case "Forest":
				Attributes.Add (RegionAttribute.Forest);
				break;

			case "Glacier":
				Attributes.Add (RegionAttribute.Glacier);
				break;

			case "Grassland":
				Attributes.Add (RegionAttribute.Grassland);
				break;

			case "Ice Cap":
				Attributes.Add (RegionAttribute.IceCap);
				break;

			case "Rainforest":
				Attributes.Add (RegionAttribute.Rainforest);

				if (AverageTemperature > 20)
					Attributes.Add (RegionAttribute.Jungle);
				break;

			case "Taiga":
				Attributes.Add (RegionAttribute.Taiga);
				break;

			case "Tundra":
				Attributes.Add (RegionAttribute.Tundra);
				break;
			}
		}
	}
}

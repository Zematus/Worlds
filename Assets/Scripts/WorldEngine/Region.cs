using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Linq;

public class RegionAttribute {

	public delegate int GetRandomIntDelegate (int maxValue);

	public string Name;

	public List<string> Variations;

	public static RegionAttribute Glacier = new RegionAttribute ("Glacier", new string[] {"glacier"});
	public static RegionAttribute IceCap = new RegionAttribute ("IceCap", new string[] {"ice cap"});
	public static RegionAttribute Ocean = new RegionAttribute ("Ocean", new string[] {"ocean"});
	public static RegionAttribute Grassland = new RegionAttribute ("Grassland", new string[] {"grass:land{:[npl]s}", "steppe{:[npl]s}", "savanna{:[npl]s}", "shrub:land{:[npl]s}", "prairie{:[npl]s}", "range{:[npl]s}"});
	public static RegionAttribute Forest = new RegionAttribute ("Forest", new string[] {"forest", "wood{:land}{:[npl]s}"});
	public static RegionAttribute Taiga = new RegionAttribute ("Taiga", new string[] {"taiga", "hinter:land{:[npl]s}", "snow forest", "snow wood{:land}{:[npl]s}"});
	public static RegionAttribute Tundra = new RegionAttribute ("Tundra", new string[] {"tundra", "waste{:land}{:[npl]s}", "frozen land{:[npl]s}", "frozen expanse"});
	public static RegionAttribute Desert = new RegionAttribute ("Desert", new string[] {"desert"});
	public static RegionAttribute Rainforest = new RegionAttribute ("Rainforest", new string[] {"rain:forest"});
	public static RegionAttribute Jungle = new RegionAttribute ("Jungle", new string[] {"jungle"});
	public static RegionAttribute Valley = new RegionAttribute ("Valley", new string[] {"valley"});
	public static RegionAttribute Highland = new RegionAttribute ("Highland", new string[] {"high:land{:[npl]s}"});
	public static RegionAttribute MountainRange = new RegionAttribute ("MountainRange", new string[] {"mountain range", "mountain:[npl]s", "mount:[npl]s"});
	public static RegionAttribute Hill = new RegionAttribute ("Hill", new string[] {"hill{:[npl]s}"});
	public static RegionAttribute Mountain = new RegionAttribute ("Mountain", new string[] {"mountain", "mount"});
	public static RegionAttribute Basin = new RegionAttribute ("Basin", new string[] {"basin"});
	public static RegionAttribute Plain = new RegionAttribute ("Plain", new string[] {"plain"});
	public static RegionAttribute Delta = new RegionAttribute ("Delta", new string[] {"desert"});
	public static RegionAttribute Peninsula = new RegionAttribute ("Peninsula", new string[] {"peninsula"});
	public static RegionAttribute Island = new RegionAttribute ("Island", new string[] {"island"});
	public static RegionAttribute Archipelago = new RegionAttribute ("Archipelago", new string[] {"archipelago", "island:[npl]s"});
	public static RegionAttribute Chanel = new RegionAttribute ("Chanel", new string[] {"chanel"});
	public static RegionAttribute Gulf = new RegionAttribute ("Gulf", new string[] {"gulf"});
	public static RegionAttribute Sound = new RegionAttribute ("Sound", new string[] {"sound"});
	public static RegionAttribute Lake = new RegionAttribute ("Lake", new string[] {"lake"});
	public static RegionAttribute Sea = new RegionAttribute ("Sea", new string[] {"sea"});
	public static RegionAttribute Continent = new RegionAttribute ("Continent", new string[] {"continent"});
	public static RegionAttribute Strait = new RegionAttribute ("Strait", new string[] {"strait", "pass"});
	public static RegionAttribute Coast = new RegionAttribute ("Coast", new string[] {"coast"});

	public static Dictionary<string, RegionAttribute> Attributes = new Dictionary<string, RegionAttribute> () {
		{"Glacier", Glacier},
		{"IceCap", IceCap},
		{"Ocean", Ocean},
		{"Grassland", Grassland},
		{"Forest", Forest},
		{"Taiga", Taiga},
		{"Tundra", Tundra},
		{"Desert", Desert},
		{"Rainforest", Rainforest},
		{"Jungle", Jungle},
		{"Valley", Valley},
		{"Highland", Highland},
		{"MountainRange", MountainRange},
		{"Hill", Hill},
		{"Mountain", Mountain},
		{"Basin", Basin},
		{"Plain", Plain},
		{"Delta", Delta},
		{"Peninsula", Peninsula},
		{"Island", Island},
		{"Archipelago", Archipelago},
		{"Chanel", Chanel},
		{"Gulf", Gulf},
		{"Sound", Sound},
		{"Lake", Lake},
		{"Sea", Sea},
		{"Continent", Continent},
		{"Strait", Strait},
		{"Coast", Coast}
	};

	private void GenerateVariations (string variant) {

		Match match = Language.OptionalWordPartRegex.Match (variant);

		if (!match.Success) {
		
			Variations.Add (variant);
			return;
		}

		string breakStr = (match.Groups ["break"].Success) ? match.Groups ["break"].Value : string.Empty;

		string v1 = variant.Replace (match.Value, string.Empty);
		string v2 = variant.Replace (match.Value, breakStr + match.Groups ["word"]);

		GenerateVariations (v1);
		GenerateVariations (v2);
	}

	private RegionAttribute (string name, string[] variants) {

		Name = name;

		Variations = new List<string> ();

		foreach (string variant in variants) {

			GenerateVariations (variant);
		}
	}

	public string GetRandomVariation (GetRandomIntDelegate getRandomInt) {

		int index = getRandomInt (Variations.Count);

		return Variations[index];
	}
}

public class Name : ISynchronizable {

	[XmlAttribute]
	public long LanguageId;

	[XmlAttribute]
	public string Value;
	[XmlAttribute]
	public string Meaning;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public Language Language;

	public Name () {
		
	}

	public Name (string value, string meaning, Language language, World world) {

		World = world;

		LanguageId = language.Id;
		Language = language;

		Value = value;
		Meaning = meaning;
	}

	public void Synchronize () {

	}

	public void FinalizeLoad () {

		Language = World.GetLanguage (LanguageId);

		if (Language == null) {
		
			throw new System.Exception ("Language can't be null");
		}
	}
}

public abstract class Region : ISynchronizable {

	public const float BaseMaxAltitudeDifference = 1000;
	public const int AltitudeRoundnessTarget = 2000;

	public const float MaxClosedness = 0.5f;

	[XmlAttribute]
	public long Id;

	public string Name;

	public List<string> AttributeNames = new List<string>();

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public bool IsSelected = false;

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
	public float AverageOuterBorderAltitude = 0;
	[XmlIgnore]
	public float MinAltitude = float.MaxValue;
	[XmlIgnore]
	public float MaxAltitude = float.MinValue;
	[XmlIgnore]
	public float CoastPercentage = 0;
	[XmlIgnore]
	public float OceanPercentage = 0;

	protected Dictionary<string, float> _biomePresences;

	private List<RegionAttribute> _attributes = new List<RegionAttribute>();

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

	public abstract ICollection<TerrainCell> GetCells ();

	public abstract bool IsInnerBorderCell (TerrainCell cell);

	public virtual void Synchronize () {

		if (Name == null) {
		
			throw new System.Exception ("Name can't be null");
		}

//		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		if (Name == null) {

			throw new System.Exception ("Name can't be null");
		}

		foreach (string attrName in AttributeNames) {
		
			_attributes.Add (RegionAttribute.Attributes[attrName]);
		}

//		Name.FinalizeLoad ();
	}

	public static Region TryGenerateRegion (TerrainCell startCell) {

		if (startCell.GetBiomePresence (Biome.Ocean) >= 1)
			return null;
		
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

	protected void AddAttribute (RegionAttribute attr) {
	
		AttributeNames.Add (attr.Name);
		_attributes.Add (attr);
	}

	public void GenerateName (Polity polity) {
	
		CellGroup coreGroup = polity.CoreGroup;

		if (_attributes.Count <= 0) {
		
			Name = "The Region";
			return;
		}

		List<RegionAttribute> attributes = new List<RegionAttribute> (_attributes);

		int index = coreGroup.GetNextLocalRandomInt (attributes.Count);

		RegionAttribute primaryAttribute = attributes [index];

		attributes.RemoveAt (index);

		string primaryTitle = primaryAttribute.GetRandomVariation (coreGroup.GetNextLocalRandomInt);
		primaryTitle = " " + Language.MakeFirstLetterUpper (Language.ClearConstructCharacters(primaryTitle));

		string secondaryTitle = string.Empty;

		if ((attributes.Count > 0) && (coreGroup.GetNextLocalRandomFloat () < 0.5f)) {

			index = coreGroup.GetNextLocalRandomInt (attributes.Count);

			RegionAttribute secondaryAttribute = attributes [index];

			attributes.RemoveAt (index);

			secondaryTitle = secondaryAttribute.GetRandomVariation (coreGroup.GetNextLocalRandomInt);
			secondaryTitle = " " + Language.MakeFirstLetterUpper (Language.ClearConstructCharacters(secondaryTitle));
		}

		Name = "The" + secondaryTitle + primaryTitle;
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

	private HashSet<TerrainCell> _innerBorderCells = new HashSet<TerrainCell> ();

	private HashSet<TerrainCell> _outerBorderCells = new HashSet<TerrainCell> ();

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

	public override ICollection<TerrainCell> GetCells () {

		return _cells;
	}

	public override bool IsInnerBorderCell (TerrainCell cell) {

		return _innerBorderCells.Contains (cell);
	}

	public void EvaluateAttributes () {

		Dictionary<string, float> biomePresences = new Dictionary<string, float> ();

		float oceanicArea = 0;
		float coastalOuterBorderArea = 0;
		float outerBorderArea = 0;

		foreach (TerrainCell cell in _cells) {

			float cellArea = cell.Area;

			bool isInnerBorder = false;

			bool isNotFullyOceanic = (cell.GetBiomePresence (Biome.Ocean) < 1);

			foreach (TerrainCell nCell in cell.Neighbors.Values) {

				if (nCell.Region != this) {
					isInnerBorder = true;

					if (_outerBorderCells.Add (nCell)) {

						float nCellArea = nCell.Area;

						outerBorderArea += nCellArea;
						AverageOuterBorderAltitude += cell.Altitude * nCellArea;

						if (isNotFullyOceanic && (nCell.GetBiomePresence (Biome.Ocean) >= 1)) {
						
							coastalOuterBorderArea += nCellArea;
						}
					}
				}
			}

			if (isInnerBorder) {
				_innerBorderCells.Add (cell);
			}

			if (MinAltitude > cell.Altitude) {
				MinAltitude = cell.Altitude;
			}

			if (MaxAltitude < cell.Altitude) {
				MaxAltitude = cell.Altitude;
			}

			AverageAltitude += cell.Altitude * cellArea;
			AverageRainfall += cell.Rainfall * cellArea;
			AverageTemperature += cell.Temperature * cellArea;

			AverageSurvivability += cell.Survivability * cellArea;
			AverageForagingCapacity += cell.ForagingCapacity * cellArea;
			AverageAccessibility += cell.Accessibility * cellArea;
			AverageArability += cell.Arability * cellArea;

			AverageFarmlandPercentage += cell.FarmlandPercentage * cellArea;

			foreach (string biomeName in cell.PresentBiomeNames) {

				float presenceArea = cell.GetBiomePresence(biomeName) * cellArea;

				if (biomePresences.ContainsKey (biomeName)) {
					biomePresences [biomeName] += presenceArea;
				} else {
					biomePresences.Add (biomeName, presenceArea);
				}

				if (biomeName == Biome.Ocean.Name) {
					oceanicArea += presenceArea;
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

		OceanPercentage = oceanicArea / TotalArea;

		AverageOuterBorderAltitude /= outerBorderArea;

		CoastPercentage = coastalOuterBorderArea / outerBorderArea;

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

		DefineAttributes ();
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

		base.Synchronize ();
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

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

		if ((CoastPercentage > 0.35f) && (CoastPercentage < 0.65f)) {
			AddAttribute (RegionAttribute.Coast);

		} else if ((CoastPercentage >= 0.65f) && (CoastPercentage < 1f)) {
			AddAttribute (RegionAttribute.Peninsula);

		} else if (CoastPercentage >= 1f) {
			AddAttribute (RegionAttribute.Island);
		}

		if (AverageAltitude > (AverageOuterBorderAltitude + 200f)) {

			AddAttribute (RegionAttribute.Highland);
		}

		if (AverageAltitude < (AverageOuterBorderAltitude - 200f)) {

			AddAttribute (RegionAttribute.Valley);

			if (AverageRainfall > 1000) {

				AddAttribute (RegionAttribute.Basin);
			}
		}

		if (MostBiomePresence > 0.65f) {
		
			switch (BiomeWithMostPresence) {

			case "Desert":
				AddAttribute (RegionAttribute.Desert);
				break;

			case "Desertic Tundra":
				AddAttribute (RegionAttribute.Desert);
				break;

			case "Forest":
				AddAttribute (RegionAttribute.Forest);
				break;

			case "Glacier":
				AddAttribute (RegionAttribute.Glacier);
				break;

			case "Grassland":
				AddAttribute (RegionAttribute.Grassland);
				break;

			case "Ice Cap":
				AddAttribute (RegionAttribute.IceCap);
				break;

			case "Rainforest":
				AddAttribute (RegionAttribute.Rainforest);

				if (AverageTemperature > 20)
					AddAttribute (RegionAttribute.Jungle);
				break;

			case "Taiga":
				AddAttribute (RegionAttribute.Taiga);
				break;

			case "Tundra":
				AddAttribute (RegionAttribute.Tundra);
				break;
			}
		}
	}
}

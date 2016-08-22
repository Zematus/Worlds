using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Linq;

public class RegionAttributeAdjective {
}

public class RegionAttributeNoun {

	public delegate int GetRandomIntDelegate (int maxValue);

	public string Name;

	public List<string> Variations;

	public static RegionAttributeNoun Glacier = new RegionAttributeNoun ("Glacier", new string[] {"glacier"});
	public static RegionAttributeNoun IceCap = new RegionAttributeNoun ("IceCap", new string[] {"[nad]ice cap"});
	public static RegionAttributeNoun Ocean = new RegionAttributeNoun ("Ocean", new string[] {"ocean"});
	public static RegionAttributeNoun Grassland = new RegionAttributeNoun ("Grassland", new string[] {"grass:land{:s}", "steppe{:s}", "savanna{:s}", "shrub:land{:s}", "prairie{:s}", "range{:s}", "field{:s}"});
	public static RegionAttributeNoun Forest = new RegionAttributeNoun ("Forest", new string[] {"forest", "wood{:s}", "wood:land{:s}"});
	public static RegionAttributeNoun Taiga = new RegionAttributeNoun ("Taiga", new string[] {"taiga", "hinter{:land}{:s}", "[nad]snow forest", "[nad]snow wood{:land}{:s}"});
	public static RegionAttributeNoun Tundra = new RegionAttributeNoun ("Tundra", new string[] {"tundra", "waste{:land}{:s}", "[adj]frozen land{:s}", "[adj]frozen expanse"});
	public static RegionAttributeNoun Desert = new RegionAttributeNoun ("Desert", new string[] {"desert", "sand{:s}"});
	public static RegionAttributeNoun Rainforest = new RegionAttributeNoun ("Rainforest", new string[] {"rain:forest"});
	public static RegionAttributeNoun Jungle = new RegionAttributeNoun ("Jungle", new string[] {"jungle"});
	public static RegionAttributeNoun Valley = new RegionAttributeNoun ("Valley", new string[] {"valley"});
	public static RegionAttributeNoun Highland = new RegionAttributeNoun ("Highland", new string[] {"high:land{:s}"});
	public static RegionAttributeNoun MountainRange = new RegionAttributeNoun ("MountainRange", new string[] {"[nad]mountain range", "mountain:s", "mount:s"});
	public static RegionAttributeNoun Hill = new RegionAttributeNoun ("Hill", new string[] {"hill{:s}"});
	public static RegionAttributeNoun Mountain = new RegionAttributeNoun ("Mountain", new string[] {"mountain", "mount"});
	public static RegionAttributeNoun Basin = new RegionAttributeNoun ("Basin", new string[] {"basin"});
	public static RegionAttributeNoun Plain = new RegionAttributeNoun ("Plain", new string[] {"plain{:s}"});
	public static RegionAttributeNoun Delta = new RegionAttributeNoun ("Delta", new string[] {"delta"});
	public static RegionAttributeNoun Peninsula = new RegionAttributeNoun ("Peninsula", new string[] {"peninsula"});
	public static RegionAttributeNoun Island = new RegionAttributeNoun ("Island", new string[] {"island"});
	public static RegionAttributeNoun Archipelago = new RegionAttributeNoun ("Archipelago", new string[] {"archipelago", "island:s"});
	public static RegionAttributeNoun Channel = new RegionAttributeNoun ("Channel", new string[] {"channel"});
	public static RegionAttributeNoun Gulf = new RegionAttributeNoun ("Gulf", new string[] {"gulf"});
	public static RegionAttributeNoun Sound = new RegionAttributeNoun ("Sound", new string[] {"sound"});
	public static RegionAttributeNoun Lake = new RegionAttributeNoun ("Lake", new string[] {"lake"});
	public static RegionAttributeNoun Sea = new RegionAttributeNoun ("Sea", new string[] {"sea"});
	public static RegionAttributeNoun Continent = new RegionAttributeNoun ("Continent", new string[] {"continent"});
	public static RegionAttributeNoun Strait = new RegionAttributeNoun ("Strait", new string[] {"strait", "pass"});
	public static RegionAttributeNoun Coast = new RegionAttributeNoun ("Coast", new string[] {"coast"});
	public static RegionAttributeNoun Region = new RegionAttributeNoun ("Region", new string[] {"region", "land{:s}"});
	public static RegionAttributeNoun Expanse = new RegionAttributeNoun ("Expanse", new string[] {"expanse"});

	public static Dictionary<string, RegionAttributeNoun> Attributes = new Dictionary<string, RegionAttributeNoun> () {
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
		{"Chanel", Channel},
		{"Gulf", Gulf},
		{"Sound", Sound},
		{"Lake", Lake},
		{"Sea", Sea},
		{"Continent", Continent},
		{"Strait", Strait},
		{"Coast", Coast},
		{"Region", Region},
		{"Expanse", Expanse}
	};

	private void GenerateVariations (string variant) {

		Match match = NamingTools.OptionalWordPartRegex.Match (variant);

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

	private RegionAttributeNoun (string name, string[] variants) {

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

public abstract class Region : ISynchronizable {

	public const float BaseMaxAltitudeDifference = 1000;
	public const int AltitudeRoundnessTarget = 2000;

	public const float MaxClosedness = 0.5f;

	[XmlAttribute]
	public long Id;

	public Name Name;

	public List<string> AttributeNames = new List<string>();

	[XmlIgnore]
	public bool IsSelected = false;

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

	private List<RegionAttributeNoun> _attributes = new List<RegionAttributeNoun>();

	public Region () {

	}

	public Region (World world) {

		World = world;

		Id = World.GenerateRegionId ();
	}

	public abstract ICollection<TerrainCell> GetCells ();

	public abstract bool IsInnerBorderCell (TerrainCell cell);

	public virtual void Synchronize () {

		if (Name == null) {
		
			throw new System.Exception ("Name can't be null");
		}

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		if (Name == null) {

			throw new System.Exception ("Name can't be null");
		}

		foreach (string attrName in AttributeNames) {
		
			_attributes.Add (RegionAttributeNoun.Attributes[attrName]);
		}

		Name.FinalizeLoad ();
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

	protected void AddAttribute (RegionAttributeNoun attr) {
	
		AttributeNames.Add (attr.Name);
		_attributes.Add (attr);
	}

	public void GenerateName (Polity polity) {
	
		CellGroup coreGroup = polity.CoreGroup;

		Language polityLanguage = polity.Culture.Language;

		string untranslatedName;
		Language.NounPhrase namePhrase;

		if (_attributes.Count <= 0) {

			untranslatedName = "the region";
			namePhrase = polityLanguage.TranslateNounPhrase (untranslatedName, coreGroup.GetNextLocalRandomFloat);

			Name = new Name (namePhrase, untranslatedName, polityLanguage, World);

			return;
		}

		List<RegionAttributeNoun> attributes = new List<RegionAttributeNoun> (_attributes);

		int index = coreGroup.GetNextLocalRandomInt (attributes.Count);

		RegionAttributeNoun primaryAttribute = attributes [index];

		attributes.RemoveAt (index);

		string primaryTitle = primaryAttribute.GetRandomVariation (coreGroup.GetNextLocalRandomInt);

		string secondaryTitle = string.Empty;

		if ((attributes.Count > 0) && (coreGroup.GetNextLocalRandomFloat () < 0.5f)) {

			index = coreGroup.GetNextLocalRandomInt (attributes.Count);

			RegionAttributeNoun secondaryAttribute = attributes [index];

			attributes.RemoveAt (index);

			secondaryTitle = "[nad]" + secondaryAttribute.GetRandomVariation (coreGroup.GetNextLocalRandomInt) + " ";
		}

		untranslatedName = "the " + secondaryTitle + primaryTitle;
		namePhrase = polityLanguage.TranslateNounPhrase (untranslatedName, coreGroup.GetNextLocalRandomFloat);

		Name = new Name (namePhrase, untranslatedName, polityLanguage, World);
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
			AddAttribute (RegionAttributeNoun.Coast);

		} else if ((CoastPercentage >= 0.65f) && (CoastPercentage < 1f)) {
			AddAttribute (RegionAttributeNoun.Peninsula);

		} else if (CoastPercentage >= 1f) {
			AddAttribute (RegionAttributeNoun.Island);
		}

		if (AverageAltitude > (AverageOuterBorderAltitude + 200f)) {

			AddAttribute (RegionAttributeNoun.Highland);
		}

		if (AverageAltitude < (AverageOuterBorderAltitude - 200f)) {

			AddAttribute (RegionAttributeNoun.Valley);

			if (AverageRainfall > 1000) {

				AddAttribute (RegionAttributeNoun.Basin);
			}
		}

		if (MostBiomePresence > 0.65f) {
		
			switch (BiomeWithMostPresence) {

			case "Desert":
				AddAttribute (RegionAttributeNoun.Desert);
				break;

			case "Desertic Tundra":
				AddAttribute (RegionAttributeNoun.Desert);
				break;

			case "Forest":
				AddAttribute (RegionAttributeNoun.Forest);
				break;

			case "Glacier":
				AddAttribute (RegionAttributeNoun.Glacier);
				break;

			case "Grassland":
				AddAttribute (RegionAttributeNoun.Grassland);
				break;

			case "Ice Cap":
				AddAttribute (RegionAttributeNoun.IceCap);
				break;

			case "Rainforest":
				AddAttribute (RegionAttributeNoun.Rainforest);

				if (AverageTemperature > 20)
					AddAttribute (RegionAttributeNoun.Jungle);
				break;

			case "Taiga":
				AddAttribute (RegionAttributeNoun.Taiga);
				break;

			case "Tundra":
				AddAttribute (RegionAttributeNoun.Tundra);
				break;
			}
		}
	}
}

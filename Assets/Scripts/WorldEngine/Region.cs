using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Linq;
using System;

public class RegionConstraint {

	public string Type;
	public object Value;

	public static Regex ConstraintRegex = new Regex (@"^(?<type>[\w_]+):(?<value>.+)$");

	private RegionConstraint (string type, object value) {

		Type = type;
		Value = value;
	}

	public static RegionConstraint BuildConstraint (string constraint) {

		Match match = ConstraintRegex.Match (constraint);

		if (!match.Success)
			throw new System.Exception ("Unparseable constraint: " + constraint);

		string type = match.Groups ["type"].Value;
		string valueStr = match.Groups ["value"].Value;

		switch (type) {

		case "altitude_above":
			float altitude_above = float.Parse (valueStr);

			return new RegionConstraint (type, altitude_above);

		case "altitude_below":
			float altitude_below = float.Parse (valueStr);

			return new RegionConstraint (type, altitude_below);

		case "rainfall_above":
			float rainfall_above = float.Parse (valueStr);

			return new RegionConstraint (type, rainfall_above);

		case "rainfall_below":
			float rainfall_below = float.Parse (valueStr);

			return new RegionConstraint (type, rainfall_below);

		case "temperature_above":
			float temperature_above = float.Parse (valueStr);

			return new RegionConstraint (type, temperature_above);

		case "temperature_below":
			float temperature_below = float.Parse (valueStr);

			return new RegionConstraint (type, temperature_below);

		case "any_attribute":
			string[] attributeStrs = valueStr.Split (new char[] { ',' });

			RegionAttribute[] attributes = attributeStrs.Select (s=> {

				if (!RegionAttribute.Attributes.ContainsKey (s)) {

					throw new System.Exception ("Attribute not present: " + s);
				}

				return RegionAttribute.Attributes[s];
			}).ToArray ();

			return new RegionConstraint (type, attributes);

		case "any_biome":
			string[] biomeStrs = valueStr.Split (new char[] { ',' });

			Biome[] biomes = biomeStrs.Select (s => {

				if (!Biome.Biomes.ContainsKey (s)) {

					throw new System.Exception ("Biome not present: " + s);
				}

				return Biome.Biomes[s];
			}).ToArray ();

			return new RegionConstraint (type, biomes);
		}

		throw new System.Exception ("Unhandled constraint type: " + type);
	}

	public bool Validate (Region region) {

		switch (Type) {

		case "altitude_above":
			return region.AverageAltitude >= (float)Value;

		case "altitude_below":
			return region.AverageAltitude < (float)Value;

		case "rainfall_above":
			return region.AverageRainfall >= (float)Value;

		case "rainfall_below":
			return region.AverageRainfall < (float)Value;

		case "temperature_above":
			return region.AverageTemperature >= (float)Value;

		case "temperature_below":
			return region.AverageTemperature < (float)Value;

		case "any_attribute":
			return ((RegionAttribute[])Value).Any (a => region.Attributes.Contains (a));

		case "any_biome":
			return ((Biome[])Value).Any (a => region.PresentBiomeNames.Contains (a.Name));
		}

		throw new System.Exception ("Unhandled constraint type: " + Type);
	}
}

public class RegionElement {

	public string Name;

	public string[] Adjectives;

	public RegionConstraint[] Constraints;

	public static RegionElement Stone = new RegionElement ("Stone", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "big", "light", "heavy"}, new string[] {"altitude_above:0"});
	public static RegionElement Boulder = new RegionElement ("Boulder", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "great"}, new string[] {"altitude_above:0"});
	public static RegionElement Rock = new RegionElement ("Rock", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "great", "big", "light", "heavy"}, new string[] {"altitude_above:0"});
	public static RegionElement Sand = new RegionElement ("Sand", new string[] {"white", "red", "yellow", "black", "grey", "fine", "coarse"}, new string[] {"any_attribute:Desert,Delta,Peninsula,Island,Coast"});
	public static RegionElement Tree = new RegionElement ("Tree", new string[] {"white", "red", "green", "yellow", "black", "dark", "pale", "dead", "great", "short", "wide", "tall"}, new string[] {"any_biome:Forest,Taiga,Rainforest"});
	public static RegionElement Wood = new RegionElement ("Wood", new string[] {"white", "red", "green", "yellow", "black", "dark", "pale", "dead", "hard", "soft"}, new string[] {"any_biome:Forest,Taiga,Rainforest"});
	public static RegionElement Grass = new RegionElement ("Grass", new string[] {"dead", "tall", "short", "soft", "wet", "dry"}, new string[] {"any_biome:Grassland,Tundra"});
	public static RegionElement Cloud = new RegionElement ("Cloud", new string[] {"white", "red", "black", "grey", "dark", "great", "thin", "deep", "light", "bright", "heavy"}, new string[] {"rainfall_above:675"});
	public static RegionElement Moss = new RegionElement ("Moss", new string[] {"white", "red", "green", "yellow", "black", "dark", "wet"}, new string[] {"any_biome:Forest,Taiga,Tundra,Rainforest"});
	public static RegionElement Fire = new RegionElement ("Fire", new string[] {"white", "red", "green", "blue", "yellow", "bright"}, new string[] {"altitude_above:0","temperature_above:0"});
	public static RegionElement Water = new RegionElement ("Water", new string[] {"white", "green", "blue", "clear", "dark"}, new string[] {"altitude_below:0"});
	public static RegionElement Rain = new RegionElement ("Rain", new string[] {"heavy", "soft", "dark"}, new string[] {"rainfall_above:675"});
	public static RegionElement Storm = new RegionElement ("Storm", new string[] {"heavy", "dark", "great"}, new string[] {"rainfall_above:675"});
	public static RegionElement Sun = new RegionElement ("Sun", new string[] {"white", "red", "yellow", "bright", "great"}, new string[] {"rainfall_below:1775"});
	public static RegionElement Moon = new RegionElement ("Moon", new string[] {"white", "red", "blue", "dark", "bright", "great"}, new string[] {"rainfall_below:1775"});
	public static RegionElement Day = new RegionElement ("Day", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great", "short", "long", "somber", "cheerful"}, new string[] {});
	public static RegionElement Night = new RegionElement ("Night", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great", "short", "long", "somber", "cheerful"}, new string[] {});
	public static RegionElement Air = new RegionElement ("Air", new string[] {}, new string[] {});
	public static RegionElement Sky = new RegionElement ("Sky", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great"}, new string[] {"altitude_above:3000"});
	public static RegionElement Ice = new RegionElement ("Ice", new string[] {"clear", "dark", "blue", "opaque"}, new string[] {"temperature_below:0"});
	public static RegionElement Snow = new RegionElement ("Snow", new string[] {"clear", "grey", "soft", "hard", "wet"}, new string[] {"temperature_below:5"});
	public static RegionElement Peat = new RegionElement ("Peat", new string[] {"red", "green", "yellow", "black", "grey", "dark", "wet", "dry"}, new string[] {"altitude_above:0","temperature_above:0","rainfall_above:675"});
	public static RegionElement Thunder = new RegionElement ("Thunder", new string[] {"great", "loud"}, new string[] {"rainfall_above:675"});
	public static RegionElement Lighting = new RegionElement ("Lighting", new string[] {"white", "yellow", "green", "great"}, new string[] {"rainfall_above:675"});
	public static RegionElement Mud = new RegionElement ("Mud", new string[] {"red", "green", "yellow", "black", "grey", "dark", "wet", "dry"}, new string[] {"rainfall_above:675"});
	public static RegionElement Dew = new RegionElement ("Dew", new string[] {}, new string[] {"rainfall_above:675"});
	public static RegionElement Dust = new RegionElement ("Dust", new string[] {"white", "red", "yellow", "black", "grey", "fine", "coarse"}, new string[] {"rainfall_below:675"});

	public static Dictionary<string, RegionElement> Elements = new Dictionary<string, RegionElement> () {
		{"Stone", Stone},
		{"Boulder", Boulder},
		{"Rock", Rock},
		{"Sand", Sand},
		{"Tree", Tree},
		{"Wood", Wood},
		{"Grass", Grass},
		{"Cloud", Cloud},
		{"Moss", Moss},
		{"Fire", Fire},
		{"Water", Water},
		{"Rain", Rain},
		{"Storm", Storm},
		{"Sun", Sun},
		{"Moon", Moon},
		{"Day", Day},
		{"Night", Night},
		{"Air", Air},
		{"Sky", Sky},
		{"Ice", Ice},
		{"Snow", Snow},
		{"Peat", Peat},
		{"Thunder", Thunder},
		{"Lighting", Lighting},
		{"Mud", Mud},
		{"Dew", Dew},
		{"Dust", Dust}
	};

	private RegionElement (string name, string[] adjectives, string[] constraints) {

		Name = name;

		Adjectives = adjectives;

		Constraints = new RegionConstraint[constraints.Length];

		int index = 0;
		foreach (string constraint in constraints) {
		
			Constraints [index] = RegionConstraint.BuildConstraint (constraint);
			index++;
		}
	}

	public bool Assignable (Region region) {
	
		return Constraints.All (c => c.Validate (region));
	}
}

public class RegionAttribute {

	public string Name;

	public string[] Adjectives;

	public List<string> Variations = new List<string> ();

	public static RegionAttribute Glacier = new RegionAttribute ("Glacier", new string[] {"clear", "white", "blue", "grey"}, new string[] {"glacier"});
	public static RegionAttribute IceCap = new RegionAttribute ("IceCap", new string[] {"clear", "white", "blue", "grey"}, new string[] {"[nad]ice cap"});
	public static RegionAttribute Ocean = new RegionAttribute ("Ocean", new string[] {"clear", "dark", "blue", "red", "green", "grey"}, new string[] {"ocean"});
	public static RegionAttribute Grassland = new RegionAttribute ("Grassland", new string[] {"dark", "pale", "red", "green", "grey", "yellow"}, new string[] {"grass:land{:s}", "steppe{:s}", "savanna{:s}", "shrub:land{:s}", "prairie{:s}", "range{:s}", "field{:s}"});
	public static RegionAttribute Forest = new RegionAttribute ("Forest", new string[] {"black", "dark", "pale", "red", "blue", "grey"}, new string[] {"forest", "wood{:s}", "wood:land{:s}"});
	public static RegionAttribute Taiga = new RegionAttribute ("Taiga", new string[] {"white", "black", "dark", "pale", "red", "blue", "grey"}, new string[] {"taiga", "hinter{:land}{:s}", "[nad]snow forest", "[nad]snow wood{:land}{:s}"});
	public static RegionAttribute Tundra = new RegionAttribute ("Tundra", new string[] {"white", "black", "dark", "pale", "red", "blue", "grey"}, new string[] {"tundra", "waste{:land}{:s}", "[adj]frozen land{:s}", "[adj]frozen expanse"});
	public static RegionAttribute Desert = new RegionAttribute ("Desert", new string[] {"white", "red", "yellow", "black", "grey"}, new string[] {"desert", "sand{:s}"});
	public static RegionAttribute Rainforest = new RegionAttribute ("Rainforest", new string[] {"black", "dark", "red", "blue", "grey"}, new string[] {"rain:forest"});
	public static RegionAttribute Jungle = new RegionAttribute ("Jungle", new string[] {"black", "dark", "red", "blue", "grey"}, new string[] {"jungle"});
	public static RegionAttribute Valley = new RegionAttribute ("Valley", new string[] {}, new string[] {"valley"});
	public static RegionAttribute Highland = new RegionAttribute ("Highland", new string[] {}, new string[] {"high:land{:s}"});
//	public static RegionAttribute MountainRange = new RegionAttribute ("MountainRange", new string[] {"[nad]mountain range", "mountain:s", "mount:s"});
//	public static RegionAttribute Hill = new RegionAttribute ("Hill", new string[] {"hill{:s}"});
//	public static RegionAttribute Mountain = new RegionAttribute ("Mountain", new string[] {"mountain", "mount"});
	public static RegionAttribute Basin = new RegionAttribute ("Basin", new string[] {}, new string[] {"basin"});
//	public static RegionAttribute Plain = new RegionAttribute ("Plain", new string[] {"plain{:s}"});
//	public static RegionAttribute Delta = new RegionAttribute ("Delta", new string[] {"delta"});
	public static RegionAttribute Peninsula = new RegionAttribute ("Peninsula", new string[] {}, new string[] {"peninsula"});
	public static RegionAttribute Island = new RegionAttribute ("Island", new string[] {}, new string[] {"island"});
//	public static RegionAttribute Archipelago = new RegionAttribute ("Archipelago", new string[] {"archipelago", "island:s"});
//	public static RegionAttribute Channel = new RegionAttribute ("Channel", new string[] {"channel"});
//	public static RegionAttribute Gulf = new RegionAttribute ("Gulf", new string[] {"gulf"});
//	public static RegionAttribute Sound = new RegionAttribute ("Sound", new string[] {"sound"});
//	public static RegionAttribute Lake = new RegionAttribute ("Lake", new string[] {"lake"});
//	public static RegionAttribute Sea = new RegionAttribute ("Sea", new string[] {"sea"});
//	public static RegionAttribute Continent = new RegionAttribute ("Continent", new string[] {"continent"});
//	public static RegionAttribute Strait = new RegionAttribute ("Strait", new string[] {"strait", "pass"});
	public static RegionAttribute Coast = new RegionAttribute ("Coast", new string[] {}, new string[] {"coast"});
//	public static RegionAttribute Region = new RegionAttribute ("Region", new string[] {"region", "land{:s}"});
//	public static RegionAttribute Expanse = new RegionAttribute ("Expanse", new string[] {"expanse"});

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
//		{"MountainRange", MountainRange},
//		{"Hill", Hill},
//		{"Mountain", Mountain},
		{"Basin", Basin},
//		{"Plain", Plain},
//		{"Delta", Delta},
		{"Peninsula", Peninsula},
		{"Island", Island},
//		{"Archipelago", Archipelago},
//		{"Chanel", Channel},
//		{"Gulf", Gulf},
//		{"Sound", Sound},
//		{"Lake", Lake},
//		{"Sea", Sea},
//		{"Continent", Continent},
//		{"Strait", Strait},
		{"Coast", Coast}
//		{"Region", Region}
//		{"Expanse", Expanse}
	};

	private void GenerateVariations (string variant) {

		Match match = NamingTools.OptionalWordPartRegex.Match (variant);

		if (!match.Success) {
		
			Variations.Add (variant);
			return;
		}

		string v1 = variant.Replace (match.Value, string.Empty);
		string v2 = variant.Replace (match.Value, match.Groups ["word"].Value);

		GenerateVariations (v1);
		GenerateVariations (v2);
	}

	private RegionAttribute (string name, string[] adjectives, string[] variants) {

		Name = name;

		Adjectives = adjectives;

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

	[XmlIgnore]
	public List<RegionAttribute> Attributes = new List<RegionAttribute>();

	[XmlIgnore]
	public List<RegionElement> Elements = new List<RegionElement>();

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

	public Region () {

	}

	public Region (World world, long id) {

		World = world;

//		Id = World.GenerateRegionId ();
		Id = id;
	}

	public abstract ICollection<TerrainCell> GetCells ();

	public abstract bool IsInnerBorderCell (TerrainCell cell);

	public virtual void Synchronize () {

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
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

		CellRegion region = new CellRegion (startCell);

		foreach (TerrainCell cell in acceptedCells) {

			region.AddCell (cell);
		}

		region.EvaluateAttributes ();

		return region;
	}

	protected void AddAttribute (RegionAttribute attr) {

		Attributes.Add (attr);
	}

	public string GetRandomAttributeVariation (GetRandomIntDelegate getRandomInt) {

		if (Attributes.Count <= 0) {

			return string.Empty;
		}

		int index = getRandomInt (Attributes.Count);

		return Attributes [index].GetRandomVariation (getRandomInt);
	}

	protected void AddElement (RegionElement elem) {

		Elements.Add (elem);
	}

	protected void AddElements (IEnumerable<RegionElement> elem) {

		Elements.AddRange (elem);
	}

	public void GenerateName (Polity polity, TerrainCell startCell) {

		int rngOffset = RngOffsets.REGION_GENERATE_NAME + (int)polity.Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => startCell.GetNextLocalRandomInt (rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => startCell.GetNextLocalRandomFloat (rngOffset++);

		Language polityLanguage = polity.Culture.Language;

		string untranslatedName;
		Language.Phrase namePhrase;

		if (Attributes.Count <= 0) {

			untranslatedName = "[NP](region)";

			namePhrase = polityLanguage.TranslatePhrase (untranslatedName, getRandomFloat);

			Name = new Name (namePhrase, untranslatedName, polityLanguage, World);

			return;
		}

		List<RegionAttribute> attributeNouns = new List<RegionAttribute> (Attributes);

		int index = getRandomInt (attributeNouns.Count);

		RegionAttribute primaryAttribute = attributeNouns [index];

		attributeNouns.RemoveAt (index);

		string primaryTitle = primaryAttribute.GetRandomVariation (getRandomInt);

		string secondaryTitle = string.Empty;

		if ((attributeNouns.Count > 0) && (getRandomFloat () < 0.5f)) {

			index = getRandomInt (attributeNouns.Count);

			RegionAttribute secondaryAttribute = attributeNouns [index];

			attributeNouns.RemoveAt (index);

			secondaryTitle = "[nad]" + secondaryAttribute.GetRandomVariation (getRandomInt) + " ";
		}

		untranslatedName = "[NP](" + secondaryTitle + primaryTitle + ")";
		namePhrase = polityLanguage.TranslatePhrase (untranslatedName, getRandomFloat);

		Name = new Name (namePhrase, untranslatedName, polityLanguage, World);

//		#if DEBUG
//		Debug.Log ("Region #" + Id + " name: " + Name);
//		#endif
	}

	public abstract TerrainCell GetMostCenteredCell ();
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

	private TerrainCell _mostCenteredCell = null;

	public CellRegion () {

	}

	public CellRegion (TerrainCell startCell) : base (startCell.World, startCell.GenerateUniqueIdentifier ()) {
		
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

		CalculateMostCenteredCell ();

		DefineAttributes ();
		DefineElements ();
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

	private void DefineElements () {

		AddElements(RegionElement.Elements.Values.FindAll (e => e.Assignable (this)));
	}

	private void CalculateMostCenteredCell () {

		int centerLongitude = 0, centerLatitude = 0;

		foreach (TerrainCell cell in _cells) {

			centerLongitude += cell.Longitude;
			centerLatitude += cell.Latitude;
		}

		centerLongitude /= _cells.Count;
		centerLatitude /= _cells.Count;

		TerrainCell closestCell = null;
		int closestDistCenter = int.MaxValue;

		foreach (TerrainCell cell in _cells) {

			int distCenter = Mathf.Abs(cell.Longitude - centerLongitude) + Mathf.Abs(cell.Latitude - centerLatitude);

			if ((closestCell == null) || (distCenter < closestDistCenter)) {

				closestDistCenter = distCenter;
				closestCell = cell;
			}
		}

		_mostCenteredCell = closestCell;
	}

	public override TerrainCell GetMostCenteredCell () {
		
		return _mostCenteredCell;
	}
}

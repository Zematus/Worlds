using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Linq;
using System;

public class ElementConstraint {

	public string Type;
	public object Value;

	public static Regex ConstraintRegex = new Regex (@"^(?<type>[\w_]+):(?<value>.+)$");

	private ElementConstraint (string type, object value) {

		Type = type;
		Value = value;
	}

	public static ElementConstraint BuildConstraint (string constraint) {

		Match match = ConstraintRegex.Match (constraint);

		if (!match.Success)
			throw new System.Exception ("Unparseable constraint: " + constraint);

		string type = match.Groups ["type"].Value;
		string valueStr = match.Groups ["value"].Value;

		switch (type) {

		case "altitude_above":
			float altitude_above = float.Parse (valueStr);

			return new ElementConstraint (type, altitude_above);

		case "altitude_below":
			float altitude_below = float.Parse (valueStr);

			return new ElementConstraint (type, altitude_below);

		case "rainfall_above":
			float rainfall_above = float.Parse (valueStr);

			return new ElementConstraint (type, rainfall_above);

		case "rainfall_below":
			float rainfall_below = float.Parse (valueStr);

			return new ElementConstraint (type, rainfall_below);

		case "temperature_above":
			float temperature_above = float.Parse (valueStr);

			return new ElementConstraint (type, temperature_above);

		case "temperature_below":
			float temperature_below = float.Parse (valueStr);

			return new ElementConstraint (type, temperature_below);

		case "no_attribute":
			string[] attributeStrs = valueStr.Split (new char[] { ',' });

			RegionAttribute[] attributes = attributeStrs.Select (s=> {

				if (!RegionAttribute.Attributes.ContainsKey (s)) {

					throw new System.Exception ("Attribute not present: " + s);
				}

				return RegionAttribute.Attributes[s];
			}).ToArray ();

			return new ElementConstraint (type, attributes);

		case "any_attribute":
			attributeStrs = valueStr.Split (new char[] { ',' });

			attributes = attributeStrs.Select (s=> {

				if (!RegionAttribute.Attributes.ContainsKey (s)) {

					throw new System.Exception ("Attribute not present: " + s);
				}

				return RegionAttribute.Attributes[s];
			}).ToArray ();

			return new ElementConstraint (type, attributes);

		case "any_biome":
			string[] biomeStrs = valueStr.Split (new char[] { ',' });

			Biome[] biomes = biomeStrs.Select (s => {

				if (!Biome.Biomes.ContainsKey (s)) {

					throw new System.Exception ("Biome not present: " + s);
				}

				return Biome.Biomes[s];
			}).ToArray ();

			return new ElementConstraint (type, biomes);

		case "main_biome":
			biomeStrs = valueStr.Split (new char[] { ',' });

			biomes = biomeStrs.Select (s => {

				if (!Biome.Biomes.ContainsKey (s)) {

					throw new System.Exception ("Biome not present: " + s);
				}

				return Biome.Biomes[s];
			}).ToArray ();

			return new ElementConstraint (type, biomes);
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

		case "no_attribute":
			return !((RegionAttribute[])Value).Any (a => region.Attributes.Contains (a));

		case "any_attribute":
			return ((RegionAttribute[])Value).Any (a => region.Attributes.Contains (a));

		case "any_biome":
			return ((Biome[])Value).Any (a => region.PresentBiomeNames.Contains (a.Name));

		case "main_biome":
			return ((Biome[])Value).Any (a => region.BiomeWithMostPresence == a.Name);
		}

		throw new System.Exception ("Unhandled constraint type: " + Type);
	}
}

public class Element {

	public string Name;

	public string[] Adjectives;

	public ElementConstraint[] Constraints;

	public static Element Stone = new Element ("stone", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "big", "light", "heavy"}, new string[] {"altitude_above:0"});
	public static Element Boulder = new Element ("boulder", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "great"}, new string[] {"altitude_above:0"});
	public static Element Rock = new Element ("rock", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "great", "big", "light", "heavy"}, new string[] {"altitude_above:0"});
	public static Element Sand = new Element ("sand", new string[] {"white", "red", "yellow", "black", "grey", "fine", "coarse"}, new string[] {"any_attribute:Desert,Delta,Peninsula,Island,Coast"});
	public static Element Tree = new Element ("tree", new string[] {"white", "red", "green", "yellow", "black", "dark", "pale", "dead", "great", "short", "tall", "narrow", "wide"}, new string[] {"main_biome:Forest,Taiga,Rainforest"});
	public static Element Wood = new Element ("wood", new string[] {"white", "red", "green", "yellow", "black", "dark", "pale", "dead", "hard", "soft"}, new string[] {"main_biome:Forest,Taiga,Rainforest"});
	public static Element Grass = new Element ("grass", new string[] {"wild", "dead", "tall", "short", "soft", "wet", "dry"}, new string[] {"main_biome:Grassland,Tundra"});
	public static Element Cloud = new Element ("cloud", new string[] {"white", "red", "black", "grey", "dark", "great", "thin", "deep", "light", "bright", "heavy"}, new string[] {"rainfall_above:675"});
	public static Element Moss = new Element ("moss", new string[] {"white", "red", "green", "yellow", "black", "dark", "wet"}, new string[] {"main_biome:Forest,Taiga,Tundra,Rainforest"});
	public static Element Shrub = new Element ("shrub", new string[] {"white", "red", "green", "yellow", "black", "dark", "dry"}, new string[] {"main_biome:Grassland,Tundra,Desert"});
	public static Element Fire = new Element ("fire", new string[] {"wild", "white", "red", "green", "blue", "yellow", "bright"}, new string[] {"altitude_above:0","temperature_above:0"});
	public static Element Water = new Element ("water", new string[] {"white", "green", "blue", "clear", "dark"}, new string[] {"altitude_below:0"});
	public static Element Rain = new Element ("rain", new string[] {"heavy", "soft", "dark"}, new string[] {"rainfall_above:675"});
	public static Element Storm = new Element ("storm", new string[] {"heavy", "dark", "great"}, new string[] {"rainfall_above:675"});
	public static Element Sun = new Element ("sun", new string[] {"white", "red", "yellow", "bright", "great"}, new string[] {"rainfall_below:1775"});
	public static Element Moon = new Element ("moon", new string[] {"white", "red", "blue", "dark", "bright", "great"}, new string[] {"rainfall_below:1775"});
	public static Element Day = new Element ("day", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great", "short", "long", "somber", "cheerful"}, new string[] {});
	public static Element Night = new Element ("night", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great", "short", "long", "somber", "cheerful"}, new string[] {});
	public static Element Air = new Element ("air", new string[] {}, new string[] {});
	public static Element Wind = new Element ("wind", new string[] {"strong", "soft"}, new string[] {"no_attribute:Rainforest,Jungle,Taiga,Forest"});
	public static Element Sky = new Element ("sky", new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great"}, new string[] {"altitude_above:3000"});
	public static Element Ice = new Element ("ice", new string[] {"clear", "dark", "blue", "opaque"}, new string[] {"temperature_below:0"});
	public static Element Snow = new Element ("snow", new string[] {"clear", "grey", "soft", "hard", "wet"}, new string[] {"temperature_below:5"});
	public static Element Peat = new Element ("peat", new string[] {"red", "green", "yellow", "black", "grey", "dark", "wet", "dry"}, new string[] {"altitude_above:0","temperature_above:0","rainfall_above:675"});
	public static Element Thunder = new Element ("thunder", new string[] {"great", "loud"}, new string[] {"rainfall_above:675"});
	public static Element Lighting = new Element ("lighting", new string[] {"white", "yellow", "green", "great"}, new string[] {"rainfall_above:675"});
	public static Element Mud = new Element ("mud", new string[] {"red", "green", "yellow", "black", "grey", "dark", "wet", "dry"}, new string[] {"rainfall_above:675"});
	public static Element Dew = new Element ("dew", new string[] {}, new string[] {"rainfall_above:675"});
	public static Element Dust = new Element ("dust", new string[] {"white", "red", "yellow", "black", "grey", "fine", "coarse"}, new string[] {"rainfall_below:675"});

	public static Dictionary<string, Element> Elements = new Dictionary<string, Element> () {
		{"Stone", Stone},
		{"Boulder", Boulder},
		{"Rock", Rock},
		{"Sand", Sand},
		{"Tree", Tree},
		{"Wood", Wood},
		{"Grass", Grass},
		{"Cloud", Cloud},
		{"Moss", Moss},
		{"Shrub", Shrub},
		{"Fire", Fire},
		{"Water", Water},
		{"Rain", Rain},
		{"Storm", Storm},
		{"Sun", Sun},
		{"Moon", Moon},
		{"Day", Day},
		{"Night", Night},
		{"Air", Air},
		{"Wind", Wind},
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

	private Element (string name, string[] adjectives, string[] constraints) {

		Name = name;

		Adjectives = adjectives;

		Constraints = new ElementConstraint[constraints.Length];

		int index = 0;
		foreach (string constraint in constraints) {
		
			Constraints [index] = ElementConstraint.BuildConstraint (constraint);
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

	public string[] Variations;

	public static RegionAttribute Glacier = new RegionAttribute ("Glacier", new string[] {"clear", "white", "blue", "grey"}, new string[] {"glacier"});
	public static RegionAttribute IceCap = new RegionAttribute ("IceCap", new string[] {"clear", "white", "blue", "grey"}, new string[] {"[nad]ice cap"});
	public static RegionAttribute Ocean = new RegionAttribute ("Ocean", new string[] {"clear", "dark", "blue", "red", "green", "grey"}, new string[] {"ocean"});
	public static RegionAttribute Grassland = new RegionAttribute ("Grassland", new string[] {"dark", "pale", "red", "green", "grey", "yellow"}, new string[] {"grass:land{:s}", "steppe{:s}", "savanna{:s}", "shrub:land{:s}", "prairie{:s}", "range{:s}", "field{:s}"});
	public static RegionAttribute Forest = new RegionAttribute ("Forest", new string[] {"black", "dark", "pale", "red", "blue", "grey"}, new string[] {"forest", "wood:s", "wood:land{:s}"});
	public static RegionAttribute Taiga = new RegionAttribute ("Taiga", new string[] {"white", "black", "dark", "pale", "red", "blue", "grey"}, new string[] {"taiga", "hinter{:land}{:s}"});
	public static RegionAttribute Tundra = new RegionAttribute ("Tundra", new string[] {"white", "black", "dark", "pale", "red", "blue", "grey"}, new string[] {"tundra", "waste{:land}{:s}"});
	public static RegionAttribute Desert = new RegionAttribute ("Desert", new string[] {"white", "red", "yellow", "black", "grey"}, new string[] {"desert", "sand{:s}"});
	public static RegionAttribute Rainforest = new RegionAttribute ("Rainforest", new string[] {"black", "dark", "red", "blue", "grey"}, new string[] {"{rain:}forest"});
	public static RegionAttribute Jungle = new RegionAttribute ("Jungle", new string[] {"black", "dark", "red", "blue", "grey"}, new string[] {"jungle"});
	public static RegionAttribute Valley = new RegionAttribute ("Valley", new string[] {}, new string[] {"valley"});
	public static RegionAttribute Highland = new RegionAttribute ("Highland", new string[] {}, new string[] {"high:land{:s}"});
//	public static RegionAttribute MountainRange = new RegionAttribute ("MountainRange", new string[] {"[nad]mountain range", "mountain:s", "mount:s"});
//	public static RegionAttribute Hill = new RegionAttribute ("Hill", new string[] {"hill{:s}"});
//	public static RegionAttribute Mountain = new RegionAttribute ("Mountain", new string[] {"mountain", "mount"});
	public static RegionAttribute Basin = new RegionAttribute ("Basin", new string[] {}, new string[] {"basin"});
//	public static RegionAttribute Plain = new RegionAttribute ("Plain", new string[] {"plain{:s}"});
	public static RegionAttribute Delta = new RegionAttribute ("Delta", new string[] {}, new string[] {"delta"});
	public static RegionAttribute Peninsula = new RegionAttribute ("Peninsula", new string[] {}, new string[] {"cape", "horn", "peninsula"});
	public static RegionAttribute Island = new RegionAttribute ("Island", new string[] {}, new string[] {"isle", "island"});
//	public static RegionAttribute Archipelago = new RegionAttribute ("Archipelago", new string[] {"archipelago", "isle:s", "island:s"});
//	public static RegionAttribute Channel = new RegionAttribute ("Channel", new string[] {"channel"});
//	public static RegionAttribute Gulf = new RegionAttribute ("Gulf", new string[] {"gulf"});
//	public static RegionAttribute Sound = new RegionAttribute ("Sound", new string[] {"sound"});
//	public static RegionAttribute Lake = new RegionAttribute ("Lake", new string[] {"lake"});
//	public static RegionAttribute Sea = new RegionAttribute ("Sea", new string[] {"sea"});
//	public static RegionAttribute Continent = new RegionAttribute ("Continent", new string[] {"continent"});
//	public static RegionAttribute Strait = new RegionAttribute ("Strait", new string[] {"strait", "pass"});
	public static RegionAttribute Coast = new RegionAttribute ("Coast", new string[] {}, new string[] {"strand", "coast"});
	public static RegionAttribute Region = new RegionAttribute ("Region", new string[] {"dark", "bleak", "open"}, new string[] {"region", "land{:s}"});
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
		{"Delta", Delta},
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
		{"Coast", Coast},
		{"Region", Region}
//		{"Expanse", Expanse}
	};

	private RegionAttribute (string name, string[] adjectives, string[] variants) {

		Name = name;

		Adjectives = adjectives;

		Variations = NamingTools.GenerateNounVariations (variants);
	}

	public string GetRandomVariation (GetRandomIntDelegate getRandomInt, Element filterElement = null) {

		IEnumerable<string> filteredVariations = Variations;

		if (filterElement != null) {
			filteredVariations = Variations.Where (s => !s.Contains (filterElement.Name));
		}

		return filteredVariations.RandomSelect (getRandomInt);
	}

	public string GetRandomVariation (GetRandomIntDelegate getRandomInt, string filterStr) {

		IEnumerable<string> filteredVariations = Variations;

		filterStr = filterStr.ToLower ();

		if (filterStr != null) {
			filteredVariations = Variations.Where (s => !s.Contains (filterStr));
		}

		return filteredVariations.RandomSelect (getRandomInt);
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
	public List<Element> Elements = new List<Element>();

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

		HashSet<CellRegion> borderingRegions = new HashSet<CellRegion> ();

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

//		float maxClosedness = 0;

		while (addedAcceptedCells) {
			HashSet<TerrainCell> nextCellsToExplore = new HashSet<TerrainCell> ();
			addedAcceptedCells = false;

			if (cellsToExplore.Count <= 0)
				break;

			float closedness = 1 - cellsToExplore.Count / (float)(cellsToExplore.Count + borderCells);

//			if (closedness > maxClosedness)
//				maxClosedness = closedness;

			foreach (TerrainCell cell in cellsToExplore) {

				float closednessFactor = 1;
				float cutOffFactor = 2;

				if (MaxClosedness < 1) {
					closednessFactor = (1 + MaxClosedness / cutOffFactor) * (1 - closedness) / (1 - MaxClosedness) - MaxClosedness / cutOffFactor;
				}

				float maxAltitudeDifference = BaseMaxAltitudeDifference * closednessFactor;

				bool accepted = false;

				string cellBiomeName = cell.BiomeWithMostPresence;

				if (cell.Region != null) {
				
					borderingRegions.Add (cell.Region as CellRegion);

				} else if (cellBiomeName == biomeName) {

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

		CellRegion region = null;

		if ((regionSize <= 20) && (borderingRegions.Count > 0)) {

			int rngOffset = RngOffsets.REGION_SELECT_BORDER_REGION;

			GetRandomIntDelegate getRandomInt = (int maxValue) => startCell.GetNextLocalRandomInt (rngOffset++, maxValue);

			region = borderingRegions.RandomSelect (getRandomInt);

		} else {
			
			region = new CellRegion (startCell);
		}

		foreach (TerrainCell cell in acceptedCells) {

			region.AddCell (cell);
		}

		foreach (CellRegion bRegion in borderingRegions) {
		
			region.AddBorderingRegion (bRegion);
		}

		region.EvaluateAttributes ();

		region.Update ();

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

	protected void AddElement (Element elem) {

		Elements.Add (elem);
	}

	protected void AddElements (IEnumerable<Element> elem) {

		Elements.AddRange (elem);
	}

	public string GetRandomUnstranslatedAreaName (GetRandomIntDelegate getRandomInt, bool isNounAdjunct) {

		string untranslatedName;

		Element element = Elements.RandomSelect (getRandomInt, isNounAdjunct ? 5 : 20);

		List<RegionAttribute> remainingAttributes = new List<RegionAttribute> (Attributes);

		RegionAttribute attribute = remainingAttributes.RandomSelectAndRemove (getRandomInt);

		IEnumerable<string> possibleAdjectives = attribute.Adjectives;

		bool addAttributeNoun = true;

		int wordCount = 0;

		if (element != null) {
			possibleAdjectives = element.Adjectives;

			wordCount++;

			if (isNounAdjunct && (getRandomInt (10) > 4)) {

				addAttributeNoun = false;
			}
		}

		string attributeNoun = string.Empty;

		if (addAttributeNoun) {
			attributeNoun = attribute.GetRandomVariation (getRandomInt, element);

			wordCount++;
		}

		int nullAdjectives = 4 * wordCount * (isNounAdjunct ? 4 : 1);

		string adjective = possibleAdjectives.RandomSelect (getRandomInt, nullAdjectives);
		if (!string.IsNullOrEmpty (adjective))
			adjective = "[adj]" + adjective + " ";

		string elementNoun = string.Empty;
		if (element != null)
			elementNoun = "[nad]" + element.Name + ((addAttributeNoun) ? " " : string.Empty);

		untranslatedName = adjective + elementNoun;

		if (isNounAdjunct) {
			untranslatedName += (addAttributeNoun) ? ("[nad]" + attributeNoun) : string.Empty;
		} else {
			untranslatedName += attributeNoun;
		}

		return untranslatedName;
	}

	public void GenerateName (Polity polity, TerrainCell originCell) {

		int rngOffset = RngOffsets.REGION_GENERATE_NAME + (int)polity.Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => originCell.GetNextLocalRandomInt (rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => originCell.GetNextLocalRandomFloat (rngOffset++);

		Language polityLanguage = polity.Culture.Language;

		string untranslatedName;
		Language.Phrase namePhrase;

		int wordCount = 1;

		List<RegionAttribute> remainingAttributes = new List<RegionAttribute> (Attributes);

		RegionAttribute primaryAttribute = remainingAttributes.RandomSelectAndRemove (getRandomInt);

		List<Element> remainingElements = new List<Element> (Elements);

		Element firstElement = remainingElements.RandomSelect (getRandomInt, 5, true);

		IEnumerable<string> possibleAdjectives = primaryAttribute.Adjectives;

		if (firstElement != null) {
			possibleAdjectives = firstElement.Adjectives;

			wordCount++;
		}

		string primaryAttributeNoun = primaryAttribute.GetRandomVariation (getRandomInt, firstElement);

		string secondaryAttributeNoun = string.Empty;

		int elementFactor = (firstElement != null) ? 8 : 4;

		float secondaryAttributeChance = 4f / (elementFactor + possibleAdjectives.Count ());

		if ((remainingAttributes.Count > 0) && (getRandomFloat () < secondaryAttributeChance)) {

			RegionAttribute secondaryAttribute = remainingAttributes.RandomSelectAndRemove (getRandomInt);

			if (firstElement == null) {
				possibleAdjectives = possibleAdjectives.Union (secondaryAttribute.Adjectives);
			}

			secondaryAttributeNoun = "[nad]" + secondaryAttribute.GetRandomVariation (getRandomInt, firstElement) + " ";

			wordCount++;
		}

		string adjective = possibleAdjectives.RandomSelect (getRandomInt, (int)Mathf.Pow (2, wordCount));

		if (!string.IsNullOrEmpty (adjective))
			adjective = "[adj]" + adjective + " ";

		string elementNoun = string.Empty;
		if (firstElement != null) {
			elementNoun = "[nad]" + firstElement.Name + " ";
		}

		untranslatedName = "[NP](" + adjective + elementNoun + secondaryAttributeNoun + primaryAttributeNoun + ")";
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

	public List<long> BorderingRegionIds;

	private HashSet<TerrainCell> _cells = new HashSet<TerrainCell> ();

	private HashSet<TerrainCell> _innerBorderCells = new HashSet<TerrainCell> ();

	private HashSet<TerrainCell> _outerBorderCells = new HashSet<TerrainCell> ();

	private TerrainCell _mostCenteredCell = null;

	private HashSet<CellRegion> _borderingRegions = new HashSet<CellRegion> ();

	public CellRegion () {

	}

	public CellRegion (TerrainCell startCell) : base (startCell.World, startCell.GenerateUniqueIdentifier ()) {
		
	}

	public void Update () {
	
		foreach (TerrainCell cell in _cells) {
			Manager.AddUpdatedCell (cell, CellUpdateType.Region);
		}
	}

	public bool AddCell (TerrainCell cell) {

		if (!_cells.Add (cell))
			return false;

		cell.Region = this;
//		Manager.AddUpdatedCell (cell, CellUpdateType.Region);

		return true;
	}

	public bool AddBorderingRegion (CellRegion region) {

		if (!_borderingRegions.Add (region))
			return false;

		region.AddBorderingRegion (this);

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

		_innerBorderCells.Clear ();
		_outerBorderCells.Clear ();

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

		BorderingRegionIds = new List<long> (_borderingRegions.Count);

		foreach (CellRegion region in _borderingRegions) {

			BorderingRegionIds.Add (region.Id);
		}

		base.Synchronize ();
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		foreach (int regionId in BorderingRegionIds) {

			CellRegion region = World.GetRegion (regionId) as CellRegion;

			if (region == null) {
				throw new System.Exception ("CellRegion missing, Id: " + regionId);
			}

			_borderingRegions.Add (region);
		}

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

		Attributes.Clear ();

		if ((CoastPercentage > 0.45f) && (CoastPercentage < 0.70f)) {
			AddAttribute (RegionAttribute.Coast);

		} else if ((CoastPercentage >= 0.70f) && (CoastPercentage < 1f)) {
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

		if (Attributes.Count <= 0) {
			AddAttribute (RegionAttribute.Region);
		}
	}

	private void DefineElements () {

		Elements.Clear ();

		AddElements(Element.Elements.Values.Where (e => e.Assignable (this)));
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

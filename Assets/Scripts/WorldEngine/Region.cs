using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Linq;
using System;

public class RegionAttribute {

	public const string RelationTag = "relation";

	public string Name;

	public string[] Adjectives;

	public Variation[] Variations;

	public Association[] Associations;

	public static RegionAttribute Glacier = new RegionAttribute ("Glacier", 
		new string[] {"clear", "white", "blue", "grey"}, 
		new string[] {"glacier{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute IceCap = new RegionAttribute ("IceCap", 
		new string[] {"clear", "white", "blue", "grey"}, 
		new string[] {"[nad]ice cap{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Ocean = new RegionAttribute ("Ocean", 
		new string[] {"clear", "dark", "blue", "red", "green", "grey"}, 
		new string[] {"ocean{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Grassland = new RegionAttribute ("Grassland", 
		new string[] {"dark", "pale", "red", "green", "grey", "yellow"}, 
		new string[] {"grass:land{:s}", "steppe{:s}", "savanna{:s}", "shrub:land{:s}", "prairie{:s}", "range{:s}", "field{:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Forest = new RegionAttribute ("Forest", 
		new string[] {"black", "dark", "pale", "red", "blue", "grey"}, 
		new string[] {"forest{<relation>:s}", "wood:s", "wood:land{:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Taiga = new RegionAttribute ("Taiga", 
		new string[] {"white", "black", "dark", "pale", "red", "blue", "grey"}, 
		new string[] {"taiga{<relation>:s}", "hinter{:land}{:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Tundra = new RegionAttribute ("Tundra", 
		new string[] {"white", "black", "dark", "pale", "red", "blue", "grey"}, 
		new string[] {"tundra{<relation>:s}", "waste{:land}{:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Desert = new RegionAttribute ("Desert", 
		new string[] {"white", "red", "yellow", "black", "grey"}, 
		new string[] {"desert{<relation>:s}", "sand{:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Rainforest = new RegionAttribute ("Rainforest", 
		new string[] {"black", "dark", "red", "blue", "grey"}, 
		new string[] {"{rain:}forest{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Jungle = new RegionAttribute ("Jungle", 
		new string[] {"black", "dark", "red", "blue", "grey"}, 
		new string[] {"jungle{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Valley = new RegionAttribute ("Valley", 
		new string[] {}, 
		new string[] {"valley{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Highland = new RegionAttribute ("Highland", 
		new string[] {}, 
		new string[] {"high:land{:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
//	public static RegionAttribute MountainRange = new RegionAttribute ("MountainRange", new string[] {"[nad]mountain range", "mountain:s", "mount:s"});
//	public static RegionAttribute Hill = new RegionAttribute ("Hill", new string[] {"hill{:s}"});
//	public static RegionAttribute Mountain = new RegionAttribute ("Mountain", new string[] {"mountain", "mount"});
	public static RegionAttribute Basin = new RegionAttribute ("Basin", 
		new string[] {}, 
		new string[] {"basin{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
//	public static RegionAttribute Plain = new RegionAttribute ("Plain", new string[] {"plain{:s}"});
	public static RegionAttribute Delta = new RegionAttribute ("Delta", 
		new string[] {}, 
		new string[] {"delta{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Peninsula = new RegionAttribute ("Peninsula", 
		new string[] {}, 
		new string[] {"cape{<relation>:s}", "horn{<relation>:s}", "peninsula{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Island = new RegionAttribute ("Island", 
		new string[] {}, 
		new string[] {"isle{<relation>:s}", "island{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
//	public static RegionAttribute Archipelago = new RegionAttribute ("Archipelago", new string[] {"archipelago", "isle:s", "island:s"});
//	public static RegionAttribute Channel = new RegionAttribute ("Channel", new string[] {"channel"});
//	public static RegionAttribute Gulf = new RegionAttribute ("Gulf", new string[] {"gulf"});
//	public static RegionAttribute Sound = new RegionAttribute ("Sound", new string[] {"sound"});
//	public static RegionAttribute Lake = new RegionAttribute ("Lake", new string[] {"lake"});
//	public static RegionAttribute Sea = new RegionAttribute ("Sea", new string[] {"sea"});
//	public static RegionAttribute Continent = new RegionAttribute ("Continent", new string[] {"continent"});
//	public static RegionAttribute Strait = new RegionAttribute ("Strait", new string[] {"strait", "pass"});
	public static RegionAttribute Coast = new RegionAttribute ("Coast", 
		new string[] {}, 
		new string[] {"strand{<relation>:s}", "coast{<relation>:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
	public static RegionAttribute Region = new RegionAttribute ("Region", 
		new string[] {"dark", "bleak", "open"}, 
		new string[] {"region{<relation>:s}", "land{:s}"},
		new string[] {"[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[nrv]walk:er,of,ip|ns", "[nrv]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds"});
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

	private RegionAttribute (string name, string[] adjectives, string[] variants, string[] associationStrs) {

		Name = name;

		Adjectives = adjectives;

		Variations = NamingTools.GenerateNounVariations (variants);

		List<Association> associations = new List<Association> ();

		foreach (string assocStr in associationStrs) {

			associations.AddRange (Association.Parse (assocStr));
		}

		Associations = associations.ToArray ();
	}

	public string GetRandomVariation (GetRandomIntDelegate getRandomInt, Element filterElement = null, bool filterRelationTagged = true) {

		IEnumerable<Variation> filteredVariations = Variations;

		if (filterElement != null) {
			filteredVariations = Variations.Where (v => !v.Text.Contains (filterElement.SingularName));
		}

		if (filterRelationTagged) {
			filteredVariations = Variations.Where (v => !v.Tags.Contains (RegionAttribute.RelationTag));
		}

		return filteredVariations.RandomSelect (getRandomInt).Text;
	}

	public string GetRandomVariation (GetRandomIntDelegate getRandomInt, string filterStr, bool filterRelationTagged = true) {

		IEnumerable<Variation> filteredVariations = Variations;

		filterStr = filterStr.ToLower ();

		if (filterStr != null) {
			filteredVariations = Variations.Where (v => !v.Text.Contains (filterStr));
		}

		if (filterRelationTagged) {
			filteredVariations = Variations.Where (v => !v.Tags.Contains (RegionAttribute.RelationTag));
		}

		return filteredVariations.RandomSelect (getRandomInt).Text;
	}

	public string GetRandomSingularVariation (GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true) {

		IEnumerable<Variation> filteredVariations = Variations.Where (v => !Language.IsPluralForm(v.Text));

		if (filterRelationTagged) {
			filteredVariations = Variations.Where (v => !v.Tags.Contains (RegionAttribute.RelationTag));
		}

		return filteredVariations.RandomSelect (getRandomInt).Text;
	}

	public string GetRandomPluralVariation (GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true) {

		IEnumerable<Variation> filteredVariations = Variations.Where (v => Language.IsPluralForm(v.Text));

		if (filterRelationTagged) {
			filteredVariations = Variations.Where (v => !v.Tags.Contains (RegionAttribute.RelationTag));
		}

		return filteredVariations.RandomSelect (getRandomInt).Text;
	}

	public string GetRandomVariation (GetRandomIntDelegate getRandomInt, bool filterRelationTagged) {

		IEnumerable<Variation> filteredVariations = Variations;

		if (filterRelationTagged) {
			filteredVariations = Variations.Where (v => !v.Tags.Contains (RegionAttribute.RelationTag));
		}

		return filteredVariations.RandomSelect (getRandomInt).Text;
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
			elementNoun = "[nad]" + element.SingularName + ((addAttributeNoun) ? " " : string.Empty);

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
			elementNoun = "[nad]" + firstElement.SingularName + " ";
		}

		untranslatedName = "[Proper][NP](" + adjective + elementNoun + secondaryAttributeNoun + primaryAttributeNoun + ")";
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

	public CellRegion (TerrainCell startCell) : base (startCell.World, startCell.GenerateUniqueIdentifier (startCell.World.CurrentDate)) {
		
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

		foreach (long regionId in BorderingRegionIds) {

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

using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

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
	public float AverageAltitude;
	[XmlIgnore]
	public float AverageRainfall;
	[XmlIgnore]
	public float AverageTemperature;

	[XmlIgnore]
	public float AverageSurvivability;
	[XmlIgnore]
	public float AverageForagingCapacity;
	[XmlIgnore]
	public float AverageAccessibility;
	[XmlIgnore]
	public float AverageArability;

	[XmlIgnore]
	public float AverageFarmlandPercentage;

	[XmlIgnore]
	public float TotalArea;

	[XmlIgnore]
	public string BiomeWithMostPresence = null;
	[XmlIgnore]
	public float MostBiomePresence;

	[XmlIgnore]
	public List<string> PresentBiomeNames = new List<string>();
	[XmlIgnore]
	public List<float> BiomePresences = new List<float>();

	[XmlIgnore]
	public float AverageOuterBorderAltitude;
	[XmlIgnore]
	public float MinAltitude;
	[XmlIgnore]
	public float MaxAltitude;
	[XmlIgnore]
	public float CoastPercentage;
	[XmlIgnore]
	public float OceanPercentage;

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

//	protected void AddElement (Element elem) {
//
//		Elements.Add (elem);
//	}

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
		namePhrase = polityLanguage.TranslatePhrase (untranslatedName);

		Name = new Name (namePhrase, untranslatedName, polityLanguage, World);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
////			if ((polity.Id == Manager.TracingData.PolityId) && (originCell.Longitude == Manager.TracingData.Longitude) && (originCell.Latitude == Manager.TracingData.Latitude)) {
//				string polityId = "Id:" + polity.Id;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"Region::GenerateName - Polity: " + polityId, 
//					"CurrentDate: " + World.CurrentDate +
//					", originCell: " + originCell.Position + 
//					", Attributes: " + Attributes.Count + 
//					", Elements: " + Elements.Count + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
////			}
//		}
//		#endif

//		#if DEBUG
//		Debug.Log ("Region #" + Id + " name: " + Name);
//		#endif
	}

	public abstract TerrainCell GetMostCenteredCell ();
}

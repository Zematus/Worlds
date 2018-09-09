
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

public class Tribe : Polity {

	public const int MinPopulationForTribeCore = 500;

	public const float TribalExpansionFactor = 2f;

	public const int TribeLeaderAvgTimeSpan = 41;

	public const string PolityType = "Tribe";
    public const string PolityNameFormat = "the {0} tribe";

    private static string[] PrepositionVariations = new string[] { "from", "of" };

	private static Variation[] TribeNounVariations;

	private static string[] TribeNounVariants = new string[] { 
		"nation", "tribe", "[ipn(person)]people", "folk", "community", "kin", "{kin:s:}person:s", "{kin:s:}[ipn(man)]men", "{kin:s:}[ipn(woman)]women", "[ipn(child)]children" };

	public const float BaseCoreProminence = 0.5f;

	public Tribe () {

	}

	public Tribe (CellGroup coreGroup) : base (PolityType, coreGroup) {

		//// Make sure there's a region to spawn into

		TerrainCell coreCell = coreGroup.Cell;

		Region cellRegion = coreGroup.Cell.Region;

		if (cellRegion == null) {

//			#if DEBUG
//			if (Manager.RegisterDebugEvent != null) {
//				if ((Id == Manager.TracingData.PolityId) && (coreCell.Longitude == Manager.TracingData.Longitude) && (coreCell.Latitude == Manager.TracingData.Latitude)) {
//					bool debug = true;
//				}
//			}
//			#endif

			cellRegion = Region.TryGenerateRegion (coreCell);

			if (cellRegion != null) {
				cellRegion.GenerateName (this, coreCell);

				if (World.GetRegion (cellRegion.Id) == null)
					World.AddRegion (cellRegion);
			} else {
			
				Debug.LogError ("No region could be generated");
			}
		}

		////

		float randomValue = coreGroup.Cell.GetNextLocalRandomFloat (RngOffsets.TRIBE_GENERATE_NEW_TRIBE);
		float coreProminence = BaseCoreProminence + randomValue * (1 - BaseCoreProminence);

		coreGroup.SetPolityProminence (this, coreProminence, 0, 0);

		World.AddGroupToUpdate (coreGroup);

		GenerateName ();

//		Debug.Log ("New tribe '" + Name + "' spawned at " + coreGroup.Cell.Position);

		//// Add starting clan

		Clan clan = new Clan (this, coreGroup, 1); // Clan should be initialized when the Tribe gets initialized

		AddFaction (clan);

		SetDominantFaction (clan);
	}

	public Tribe (Clan triggerClan, Polity parentPolity) : base (PolityType, triggerClan.CoreGroup, parentPolity) {

		triggerClan.ChangePolity (this, triggerClan.Influence);

		SwitchCellProminences (parentPolity, triggerClan);

		GenerateName ();

		////

//		Debug.Log ("New tribe '" + Name + "' from tribe '" + parentPolity.Name + "' with total transfered influence = " + transferedInfluence);
	}

	public override void InitializeInternal () {

		AddEvent (new FosterTribeRelationDecisionEvent (this, FosterTribeRelationDecisionEvent.CalculateTriggerDate (this)));
		AddEvent (new MergeTribesDecisionEvent (this, MergeTribesDecisionEvent.CalculateTriggerDate (this)));
		AddEvent (new OpenTribeDecisionEvent (this, OpenTribeDecisionEvent.CalculateTriggerDate (this)));
	}

	public static void GenerateTribeNounVariations () {

		TribeNounVariations = NamingTools.GenerateNounVariations (TribeNounVariants);
	}

    protected override void GenerateEventsFromData()
    {
        foreach (PolityEventData eData in EventDataList)
        {
            switch (eData.TypeId)
            {
                case WorldEvent.FosterTribeRelationDecisionEventId:
                    AddEvent(new FosterTribeRelationDecisionEvent(this, eData));
                    break;
                case WorldEvent.MergeTribesDecisionEventId:
                    AddEvent(new MergeTribesDecisionEvent(this, eData));
                    break;
                case WorldEvent.OpenTribeDecisionEventId:
                    AddEvent(new OpenTribeDecisionEvent(this, eData));
                    break;
                default:
                    throw new System.Exception("Unhandled polity event type id: " + eData.TypeId);
            }
        }
    }

    private void SwitchCellProminences (Polity sourcePolity, Clan triggerClan) {

		float targetPolityInfluence = triggerClan.Influence;
		float sourcePolityInfluence = 1 - targetPolityInfluence;

		#if DEBUG
		if (targetPolityInfluence <= 0) {
			throw new System.Exception ("Pulling clan influence equal or less than zero.");
		}
		#endif

//		#if DEBUG
//		if (sourcePolity.Territory.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		int maxGroupCount = sourcePolity.Groups.Count;

		Dictionary<CellGroup, float> groupDistances = new Dictionary<CellGroup, float> (maxGroupCount);

		Queue<CellGroup> sourceGroups = new Queue<CellGroup> (maxGroupCount);

		sourceGroups.Enqueue (CoreGroup);

		int reviewedCells = 0;
		int switchedCells = 0;

		HashSet<Faction> factionsToTransfer = new HashSet<Faction> ();

		while (sourceGroups.Count > 0) {
		
			CellGroup group = sourceGroups.Dequeue ();

			if (groupDistances.ContainsKey (group))
				continue;

			PolityProminence pi = group.GetPolityProminence (sourcePolity);

			if (pi == null)
				continue;

			reviewedCells++;

			float distanceToTargetPolityCore = CalculateShortestCoreDistance (group, groupDistances);

			if (distanceToTargetPolityCore >= CellGroup.MaxCoreDistance)
				continue;

			groupDistances.Add (group, distanceToTargetPolityCore);

			float distanceToSourcePolityCore = pi.PolityCoreDistance;

			float percentProminence = 1f;

			if (distanceToSourcePolityCore < CellGroup.MaxCoreDistance) {

				float ditanceToCoresSum = distanceToTargetPolityCore + distanceToSourcePolityCore;
			
				float distanceFactor = distanceToSourcePolityCore / ditanceToCoresSum;

				distanceFactor = Mathf.Clamp01((distanceFactor * 3f) - 1f);

				float targetDistanceFactor = distanceFactor;
				float sourceDistanceFactor = 1 - distanceFactor;

				float targetPolityWeight = targetPolityInfluence * targetDistanceFactor;
				float sourcePolityWeight = sourcePolityInfluence * sourceDistanceFactor;

				percentProminence = targetPolityWeight / (targetPolityWeight + sourcePolityWeight);
			}

			if (percentProminence <= 0)
				continue;

			if (percentProminence > 0.5f) {
			
				switchedCells++;

				foreach (Faction faction in group.GetFactionCores ()) {

					if (faction.Polity != sourcePolity)
						continue;

//					#if DEBUG
//					if (sourcePolity.FactionCount == 1) {
//						throw new System.Exception ("Number of factions in Polity " + Id + " will be equal or less than zero. Current Date: " + World.CurrentDate);
//					}
//					#endif
				
					factionsToTransfer.Add (faction);
				}
			}

			float prominenceValue = pi.Value;
	
			group.SetPolityProminence (sourcePolity, prominenceValue * (1 - percentProminence));

			group.SetPolityProminence (this, prominenceValue * percentProminence, distanceToTargetPolityCore, distanceToTargetPolityCore);
	
			World.AddGroupToUpdate (group);

			foreach (CellGroup neighborGroup in group.Neighbors.Values) {

				if (groupDistances.ContainsKey (neighborGroup))
					continue;
			
				sourceGroups.Enqueue (neighborGroup);
			}
		}

		float highestInfluence = triggerClan.Influence;
		Clan dominantClan = triggerClan;

		foreach (Faction faction in factionsToTransfer) {

			Clan clan = faction as Clan;

			if (clan != null) {
				if (clan.Influence > highestInfluence) {
					highestInfluence = clan.Influence;
					dominantClan = clan;
				}
			}

			faction.ChangePolity (this, faction.Influence);
		}

		SetDominantFaction (dominantClan);

//		Debug.Log ("SwitchCellProminences: source polity cells: " + maxGroupCount + ", reviewed cells: " + reviewedCells + ", switched cells: " + switchedCells);
	}

	private float CalculateShortestCoreDistance (CellGroup group, Dictionary<CellGroup, float> groupDistances) {

		if (groupDistances.Count <= 0)
			return 0;

		float shortestDistance = CellGroup.MaxCoreDistance;

		foreach (KeyValuePair<Direction, CellGroup> pair in group.Neighbors) {

			float distanceToCoreFromNeighbor = float.MaxValue;

			if (!groupDistances.TryGetValue (pair.Value, out distanceToCoreFromNeighbor)) {
			
				continue;
			}

			if (distanceToCoreFromNeighbor >= float.MaxValue)
				continue;

			float neighborDistance = group.Cell.NeighborDistances[pair.Key];

			float totalDistance = distanceToCoreFromNeighbor + neighborDistance;

			if (totalDistance < 0)
				continue;

			if (totalDistance < shortestDistance)
				shortestDistance = totalDistance;
		}

		return shortestDistance;
	}

	protected override void UpdateInternal ()
	{
	}

	protected override void GenerateName ()
	{
		Region coreRegion = CoreGroup.Cell.Region;

		int rngOffset = RngOffsets.TRIBE_GENERATE_NAME + (int)Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => GetNextLocalRandomInt (rngOffset++, maxValue);
		GetRandomFloatDelegate getRandomFloat = () => GetNextLocalRandomFloat (rngOffset++);

		string tribeNoun = TribeNounVariations.RandomSelect (getRandomInt).Text;

		bool areaNameIsNounAdjunct = (getRandomFloat () > 0.5f);

		string areaName = coreRegion.GetRandomUnstranslatedAreaName (getRandomInt, areaNameIsNounAdjunct);

		string untranslatedName;

		if (areaNameIsNounAdjunct) {
			untranslatedName = "[Proper][NP](" + areaName + " " + tribeNoun + ")";
		} else {
			string preposition = PrepositionVariations.RandomSelect (getRandomInt);

			untranslatedName = "[PpPP]([Proper][NP](" + tribeNoun + ") [PP](" + preposition + " [Proper][NP](the " + areaName + ")))";
		}
        
        Info.Name = new Name(untranslatedName, Culture.Language, World);

        //		#if DEBUG
        //		Debug.Log ("Tribe #" + Id + " name: " + Name);
        //		#endif
    }

	public override float CalculateGroupProminenceExpansionValue (CellGroup sourceGroup, CellGroup targetGroup, float sourceValue)
	{
		if (sourceValue <= 0)
			return 0;

		float sourceGroupTotalPolityProminenceValue = sourceGroup.TotalPolityProminenceValue;
		float targetGroupTotalPolityProminenceValue = targetGroup.TotalPolityProminenceValue;

		if (sourceGroupTotalPolityProminenceValue <= 0) {

			throw new System.Exception ("sourceGroup.TotalPolityProminenceValue equal or less than 0: " + sourceGroupTotalPolityProminenceValue);
		}

		float prominenceFactor = sourceGroupTotalPolityProminenceValue / (targetGroupTotalPolityProminenceValue + sourceGroupTotalPolityProminenceValue);
		prominenceFactor = Mathf.Pow (prominenceFactor, 4);

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;

		CalculateAdaptionToCell (targetGroup.Cell, out modifiedForagingCapacity, out modifiedSurvivability);

		float survivabilityFactor = Mathf.Pow (modifiedSurvivability, 2);

		float finalFactor = prominenceFactor * survivabilityFactor;

		if (sourceGroup != targetGroup) {

			// There should be a strong bias against polity expansion to reduce activity
			finalFactor *= TribalExpansionFactor;
		}

		return finalFactor;
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();
	}
}

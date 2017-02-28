using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Tribe : Polity {

//	public const float TribalExpansionFactor = 0.01f;
	public const float TribalExpansionFactor = 2f;

	public const string TribeType = "Tribe";

	public static string[] TribeNounVariations = new string[] { "tribe", "people", "folk", "community", "[ipn(man)]men", "[ipn(woman)]women", "[ipn(child)]children" };

	public const float BaseCoreInfluence = 0.5f;

	public Tribe () {

	}

	public Tribe (CellGroup coreGroup) : base (TribeType, coreGroup) {

		//// Make sure there's a region to spawn into

		TerrainCell coreCell = coreGroup.Cell;

		Region cellRegion = coreGroup.Cell.Region;

		if (cellRegion == null) {

			cellRegion = Region.TryGenerateRegion (coreCell);

			if (cellRegion != null) {
				cellRegion.GenerateName (this, coreCell);

				World.AddRegion (cellRegion);
			}
		}

		////
		
		Clan clan = new Clan (coreGroup, this, 1);

		AddFaction (clan);

		SetDominantFaction (clan);

		float randomValue = coreGroup.Cell.GetNextLocalRandomFloat (RngOffsets.TRIBE_GENERATE_NEW_TRIBE);
		float coreInfluence = BaseCoreInfluence + randomValue * (1 - BaseCoreInfluence);

		coreGroup.SetPolityInfluence (this, coreInfluence);

		World.AddGroupToUpdate (coreGroup);

		GenerateName ();
	}

	public Tribe (Clan clan) : base (TribeType, clan.CoreGroup) {
		
		Polity parentPolity = clan.Polity;

		SwitchCellInfluences (parentPolity, clan);

		clan.ChangePolity (this, 1f);

		SetDominantFaction (clan);

		GenerateName ();
	}

	private void SwitchCellInfluences (Polity sourcePolity, Clan pullingClan) {
	
		float clanProminence = pullingClan.Prominence;
		float sourcePolityProminence = 1 - clanProminence;

		if (clanProminence <= 0) {
		
			throw new System.Exception ("Pulling clan prominence equal or less than zero.");
		}

		int maxGroupCount = sourcePolity.InfluencedGroups.Count;

		Dictionary<CellGroup, float> groupDistances = new Dictionary<CellGroup, float> (maxGroupCount);

		Stack<CellGroup> sourceGroups = new Stack<CellGroup> (maxGroupCount);

		sourceGroups.Push (CoreGroup);

		int reviewedCells = 0;
		int switchedCells = 0;

		while (sourceGroups.Count > 0) {
		
			CellGroup group = sourceGroups.Pop ();

			if (groupDistances.ContainsKey (group))
				continue;

			PolityInfluence pi = group.GetPolityInfluence (sourcePolity);

			if (pi == null)
				continue;

			reviewedCells++;

			float distanceToCore = CalculateShortestCoreDistance (group, groupDistances);

			if (distanceToCore >= float.MaxValue)
				continue;

			groupDistances.Add (group, distanceToCore);

			float distanceToSourcePolityCore = pi.CoreDistance;

			float percentInfluence = 1f;

			if (distanceToSourcePolityCore < float.MaxValue) {

				float totalDistanceFactor = distanceToCore + distanceToSourcePolityCore;

				if (totalDistanceFactor <= 0) {

					throw new System.Exception ("Sum of core distances equal or less than zero.");
				}
			
				float distanceFactor = distanceToSourcePolityCore / totalDistanceFactor;

				float clanWeight = clanProminence * distanceFactor;
				float sourcePolityWeight = sourcePolityProminence * (1 - distanceFactor);

				percentInfluence = clanWeight / (clanWeight + sourcePolityWeight);
			}

			if (percentInfluence > 0.5f) {
			
				switchedCells++;
			}

			if (percentInfluence <= 0)
				continue;

			float influenceValue = group.GetPolityInfluenceValue (sourcePolity);
	
			group.SetPolityInfluence (sourcePolity, influenceValue * (1 - percentInfluence));

			group.SetPolityInfluence (this, influenceValue * percentInfluence);
	
			World.AddGroupToUpdate (group);

			foreach (CellGroup neighborGroup in group.Neighbors.Values) {

				if (groupDistances.ContainsKey (neighborGroup))
					continue;
			
				sourceGroups.Push (neighborGroup);
			}
		}

		Debug.Log ("SwitchCellInfluences: source polity cells: " + maxGroupCount + ", reviewed cells: " + reviewedCells + ", switched cells: " + switchedCells);
	}

	private float CalculateShortestCoreDistance (CellGroup group, Dictionary<CellGroup, float> groupDistances) {

		if (groupDistances.Count <= 0)
			return 0;

		float shortestDistance = float.MaxValue;

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
//		TryRelocateCore ();
	}

	protected override void GenerateName ()
	{
		Region coreRegion = CoreGroup.Cell.Region;

		int rngOffset = RngOffsets.TRIBE_GENERATE_NAME + (int)Id;

		int randomInt = GetNextLocalRandomInt (rngOffset++, TribeNounVariations.Length);

		string tribeNounVariation = TribeNounVariations[randomInt];

		string regionAttributeNounVariation = coreRegion.GetRandomAttributeVariation ((int maxValue) => GetNextLocalRandomInt (rngOffset++, maxValue));

		if (regionAttributeNounVariation != string.Empty) {
			regionAttributeNounVariation = " [nad]" + regionAttributeNounVariation;
		}

		string untranslatedName = "the" + regionAttributeNounVariation + " " + tribeNounVariation;

		Language.NounPhrase namePhrase = Culture.Language.TranslateNounPhrase (untranslatedName, () => GetNextLocalRandomFloat (rngOffset++));

		Name = new Name (namePhrase, untranslatedName, Culture.Language, World);

//		#if DEBUG
//		Debug.Log ("Tribe #" + Id + " name: " + Name);
//		#endif
	}

	public override float CalculateGroupInfluenceExpansionValue (CellGroup sourceGroup, CellGroup targetGroup, float sourceValue)
	{
		if (sourceValue <= 0)
			return 0;

		float sourceGroupTotalPolityInfluenceValue = sourceGroup.TotalPolityInfluenceValue;
		float targetGroupTotalPolityInfluenceValue = targetGroup.TotalPolityInfluenceValue;

		if (sourceGroupTotalPolityInfluenceValue <= 0) {

			throw new System.Exception ("sourceGroup.TotalPolityInfluenceValue equal or less than 0: " + sourceGroupTotalPolityInfluenceValue);
		}

		float influenceFactor = sourceGroupTotalPolityInfluenceValue / (targetGroupTotalPolityInfluenceValue + sourceGroupTotalPolityInfluenceValue);
		influenceFactor = Mathf.Pow (influenceFactor, 4);

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;

		CalculateAdaptionToCell (targetGroup.Cell, out modifiedForagingCapacity, out modifiedSurvivability);

		float survivabilityFactor = Mathf.Pow (modifiedSurvivability, 2);

		float finalFactor = influenceFactor * survivabilityFactor;

		if (sourceGroup != targetGroup) {

			// There should be a strong bias against polity expansion to reduce activity
			finalFactor *= TribalExpansionFactor;
		}

		return finalFactor;
	}
}

public class TribeFormationEvent : CellGroupEvent {

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 100;

	public const int MinSocialOrganizationKnowledgeSpawnEventValue = SocialOrganizationKnowledge.MinValueForTribalismSpawnEvent;
	public const int MinSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.MinValueForTribalism;
	public const int OptimalSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.OptimalValueForTribalism;

	public const string EventSetFlag = "TribeFormationEvent_Set";

	public TribeFormationEvent () {

	}

	public TribeFormationEvent (CellGroup group, int triggerDate) : base (group, triggerDate, TribeFormationEventId) {

		Group.SetFlag (EventSetFlag);
	}

	public static int CalculateTriggerDate (CellGroup group) {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.TRIBE_FORMATION_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float socialOrganizationFactor = (socialOrganizationValue - MinSocialOrganizationKnowledgeValue) / (OptimalSocialOrganizationKnowledgeValue - MinSocialOrganizationKnowledgeValue);
		socialOrganizationFactor = Mathf.Pow (socialOrganizationFactor, 2);
		socialOrganizationFactor = Mathf.Clamp (socialOrganizationFactor, 0.001f, 1);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / socialOrganizationFactor;

		int targetDate = (int)(group.World.CurrentDate + dateSpan);

		if (targetDate <= group.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.IsFlagSet (EventSetFlag))
			return false;

		if (group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId) == null)
			return false;

		CulturalKnowledge socialOrganizationKnowledge = group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge == null)
			return false;

		if (socialOrganizationKnowledge.Value < MinSocialOrganizationKnowledgeValue)
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		CulturalDiscovery discovery = Group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId);

		if (discovery == null)
			return false;

		float influenceFactor = Mathf.Min(1, Group.TotalPolityInfluenceValue * 3f);

		if (influenceFactor >= 1)
			return false;

		if (influenceFactor <= 0)
			return true;

		influenceFactor = Mathf.Pow (1 - influenceFactor, 4);

		float triggerValue = Group.Cell.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + (int)Id);

		if (triggerValue > influenceFactor)
			return false;

		return true;
	}

	public override void Trigger () {

		Tribe tribe = new Tribe (Group);

		World.AddPolity (tribe);
		World.AddPolityToUpdate (tribe);

		World.AddGroupToUpdate (Group);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}

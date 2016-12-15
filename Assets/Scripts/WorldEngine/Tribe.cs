using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Tribe : Polity {

	public const string TribeType = "Tribe";

	public static string[] TribeNounVariations = new string[] { "tribe", "people", "folk", "community", "[ipn(man)]men", "[ipn(woman)]women", "[ipn(child)]children" };

	public const float BaseCoreInfluence = 0.5f;

	public Tribe () {

	}

	private Tribe (CellGroup coreGroup, float coreGroupInfluence) : base (TribeType, coreGroup, coreGroupInfluence) {

		AddFaction (new Clan (coreGroup, this, 1));
	}

	public static Tribe GenerateNewTribe (CellGroup coreGroup) {

		float randomValue = coreGroup.Cell.GetNextLocalRandomFloat (RngOffsets.TRIBE_GENERATE_NEW_TRIBE);
		float coreInfluence = BaseCoreInfluence + randomValue * (1 - BaseCoreInfluence);

		coreInfluence *= 1 - coreGroup.TotalPolityInfluenceValue;
	
		Tribe newTribe = new Tribe (coreGroup, coreInfluence);

		return newTribe;
	}

	public override void UpdateInternal ()
	{
		TryRelocateCore ();
	}

	protected override void GenerateName ()
	{
		Region coreRegion = CoreGroup.Cell.Region;

		int rngOffset = RngOffsets.TRIBE_GENERATE_NAME + (int)Id;

		int randomInt = CoreGroup.GetNextLocalRandomInt (rngOffset++, TribeNounVariations.Length);

		string tribeNounVariation = TribeNounVariations[randomInt];

		string regionAttributeNounVariation = coreRegion.GetRandomAttributeVariation ((int maxValue) => CoreGroup.GetNextLocalRandomInt (rngOffset++, maxValue));

		if (regionAttributeNounVariation != string.Empty) {
			regionAttributeNounVariation = " [nad]" + regionAttributeNounVariation;
		}

		string untranslatedName = "the" + regionAttributeNounVariation + " " + tribeNounVariation;

		Language.NounPhrase namePhrase = Culture.Language.TranslateNounPhrase (untranslatedName, () => CoreGroup.GetNextLocalRandomFloat (rngOffset++));

		Name = new Name (namePhrase, untranslatedName, Culture.Language, World);

//		#if DEBUG
//		Debug.Log ("Tribe #" + Id + " name: " + Name);
//		#endif
	}

	public CellGroup GetRandomWeightedInfluencedGroup (int rngOffset) {

		WeightedGroup[] weightedGroups = new WeightedGroup[InfluencedGroups.Count];

		float totalWeight = 0;

		int index = 0;
		foreach (CellGroup group in InfluencedGroups.Values) {
		
			float weight = group.Population * group.GetPolityInfluenceValue (this);

			totalWeight += weight;

			weightedGroups [index] = new WeightedGroup (group, weight);
			index++;
		}

		return CollectionUtility.WeightedSelection (weightedGroups, totalWeight, () => CoreGroup.GetNextLocalRandomFloat (rngOffset));
	}
}

public class TribeFormationEvent : CellGroupEvent {

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 1000;

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
		randomFactor = randomFactor * randomFactor;

		float socialOrganizationFactor = (socialOrganizationValue - MinSocialOrganizationKnowledgeValue) / (OptimalSocialOrganizationKnowledgeValue - MinSocialOrganizationKnowledgeValue);
		socialOrganizationFactor = Mathf.Clamp01 (socialOrganizationFactor) + 0.001f;

		float influenceFactor = group.TotalPolityInfluenceValue;
		influenceFactor = Mathf.Pow(1 - influenceFactor * 0.95f, 4);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / (socialOrganizationFactor * influenceFactor);

		int targetDate = (int)(group.World.CurrentDate + dateSpan);

		if (targetDate <= group.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.IsFlagSet (EventSetFlag))
			return false;

		if (group.Culture.GetDiscovery (TribalismDiscovery.TribalismDiscoveryId) == null)
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		CulturalDiscovery discovery = Group.Culture.GetDiscovery (TribalismDiscovery.TribalismDiscoveryId);

		if (discovery == null)
			return false;

		float influenceFactor = Mathf.Min(1, Group.TotalPolityInfluenceValue * 3f);
		influenceFactor = Mathf.Pow (1 - influenceFactor, 4);

		float triggerValue = Group.Cell.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + (int)Id);

		if (triggerValue > influenceFactor)
			return false;

		return true;
	}

	public override void Trigger () {

		World.AddPolity (Tribe.GenerateNewTribe (Group));

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

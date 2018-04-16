using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribeFormationEvent : CellGroupEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 100;

	public const int MinSocialOrganizationKnowledgeTribeFormation = SocialOrganizationKnowledge.MinValueForTribalismDiscovery;
	public const int MinSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.MinValueForHoldingTribalism;
	public const int OptimalSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.OptimalValueForTribalism;

	//	public const string EventSetFlag = "TribeFormationEvent_Set";

	public TribeFormationEvent () {

		DoNotSerialize = true;
	}

	public TribeFormationEvent (CellGroup group, long triggerDate) : base (group, triggerDate, TribeFormationEventId) {

		//		Group.SetFlag (EventSetFlag);

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (CellGroup group) {

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

		long targetDate = (long)(group.World.CurrentDate + dateSpan) + CellGroup.GenerationSpan;

		if (targetDate <= group.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.Population < Tribe.MinPopulationForTribeCore)
			return false;

		//		if (group.IsFlagSet (EventSetFlag))
		//			return false;

		if (group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId) == null)
			return false;

		CulturalKnowledge socialOrganizationKnowledge = group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge == null)
			return false;

		if (socialOrganizationKnowledge.Value < MinSocialOrganizationKnowledgeTribeFormation)
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (Group.Population < Tribe.MinPopulationForTribeCore)
			return false;

		CulturalDiscovery discovery = Group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId);

		if (discovery == null)
			return false;

		CulturalKnowledge socialOrganizationKnowledge = Group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge == null)
			return false;

		if (socialOrganizationKnowledge.Value < MinSocialOrganizationKnowledgeTribeFormation)
			return false;

		float prominenceFactor = Mathf.Min(1, Group.TotalPolityProminenceValue * 3f);

		if (prominenceFactor > 0)
			return false;

		//		if (prominenceFactor <= 0)
		//			return true;
		//
		//		prominenceFactor = Mathf.Pow (1 - prominenceFactor, 4);
		//
		//		float triggerValue = Group.Cell.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + (int)Id);
		//
		//		if (triggerValue > prominenceFactor)
		//			return false;

		return true;
	}

	public override void Trigger () {

		Territory encompassingTerritory = Group.Cell.EncompassingTerritory;

		Tribe tribe = new Tribe (Group);
		tribe.Initialize ();

		World.AddPolity (tribe);
		World.AddPolityToUpdate (tribe);

		World.AddGroupToUpdate (Group);

		PolityFormationEventMessage formationEventMessage = new PolityFormationEventMessage (tribe, TriggerDate);

		if (!World.HasEventMessage (WorldEvent.PolityFormationEventId)) {
			World.AddEventMessage (formationEventMessage);
			formationEventMessage.First = true;
		}

		if (encompassingTerritory != null) {
			encompassingTerritory.Polity.AddEventMessage (formationEventMessage);
		}
	}

	protected override void DestroyInternal ()
	{
		//		if (Group != null) {
		//			Group.UnsetFlag (EventSetFlag);
		//		}

		if (Group != null) {
			Group.HasTribeFormationEvent = false;
		}

		base.DestroyInternal ();
	}
}

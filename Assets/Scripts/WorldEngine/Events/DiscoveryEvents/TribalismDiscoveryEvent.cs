using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class TribalismDiscoveryEvent : DiscoveryEvent
{
    public const long EventMessageId = 0;
    public const string EventMessagePrefix = "Tribalism Discovered";

    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 100;

    public const int MinSocialOrganizationKnowledgeForTribalismDiscovery = SocialOrganizationKnowledge.MinValueForTribalismDiscovery;
    public const int MinSocialOrganizationKnowledgeForHoldingTribalism = SocialOrganizationKnowledge.MinValueForHoldingTribalism;
    public const int OptimalSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.TribalismDiscoveryLevelLimit;

    public const string EventSetFlag = "TribalismDiscoveryEvent_Set";

    public TribalismDiscoveryEvent()
    {

    }

    public TribalismDiscoveryEvent(CellGroup group, long triggerDate) : base(group, triggerDate, TribalismDiscoveryEventId)
    {
        Group.SetFlag(EventSetFlag);
    }

    public static long CalculateTriggerDate(CellGroup group)
    {
        int socialOrganizationValue = 0;

        group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out socialOrganizationValue);

        float randomFactor = group.Cell.GetNextLocalRandomFloat(RngOffsets.TRIBALISM_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
        randomFactor = Mathf.Pow(randomFactor, 2);

        float socialOrganizationFactor = (socialOrganizationValue - MinSocialOrganizationKnowledgeForHoldingTribalism) / (float)(OptimalSocialOrganizationKnowledgeValue - MinSocialOrganizationKnowledgeForHoldingTribalism);
        socialOrganizationFactor = Mathf.Pow(socialOrganizationFactor, 2);
        socialOrganizationFactor = Mathf.Clamp(socialOrganizationFactor, 0.001f, 1);

        float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / socialOrganizationFactor;

        long targetDate = (long)(group.World.CurrentDate + dateSpan) + 1;

        return targetDate;
    }

    public static bool CanSpawnIn(CellGroup group)
    {
        if (group.Population < Tribe.MinPopulationForTribeCore)
            return false;

        if (group.IsFlagSet(EventSetFlag))
            return false;

        if (group.Culture.HasDiscovery(TribalismDiscovery.DiscoveryId))
            return false;

        return true;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;

        if (Group.Population < Tribe.MinPopulationForTribeCore)
            return false;

        if (Group.Culture.HasDiscovery(TribalismDiscovery.DiscoveryId))
            return false;

        int value = 0;

        if (!Group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out value))
            return false;

        if (value < MinSocialOrganizationKnowledgeForTribalismDiscovery)
            return false;

        return true;
    }

    public override void Trigger()
    {
        Group.Culture.TryAddDiscoveryToFind(TribalismDiscovery.DiscoveryId);

        Tribe newTribe = null;

        if (Group.PolityProminences.Count <= 0)
        {
            newTribe = new Tribe(Group);
            newTribe.Initialize();

            World.AddPolityInfo(newTribe.Info);
            World.AddPolityToUpdate(newTribe);
        }

        World.AddGroupToUpdate(Group);

        TryGenerateEventMessages(newTribe);
    }

    protected override void DestroyInternal()
    {
        if (Group != null)
        {
            Group.UnsetFlag(EventSetFlag);
        }

        base.DestroyInternal();
    }

    public void TryGenerateEventMessages(Tribe newTribe)
    {
        TryGenerateEventMessage(TribalismDiscoveryEventId, TribalismDiscovery.DiscoveryName);

        if (newTribe != null)
        {
            PolityFormationEventMessage formationEventMessage = null;

            if (!World.HasEventMessage(WorldEvent.PolityFormationEventId))
            {
                formationEventMessage = new PolityFormationEventMessage(newTribe, TriggerDate);

                World.AddEventMessage(formationEventMessage);
                formationEventMessage.First = true;
            }

            if (Group.Cell.EncompassingTerritory != null)
            {
                Polity encompassingPolity = Group.Cell.EncompassingTerritory.Polity;

                if (formationEventMessage == null)
                {
                    formationEventMessage = new PolityFormationEventMessage(newTribe, TriggerDate);
                }

                encompassingPolity.AddEventMessage(formationEventMessage);
            }
        }
    }
}

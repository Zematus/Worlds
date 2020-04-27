using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class TribeFormationEvent : CellGroupEvent
{
    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 100;

    public const int MinSocialOrganizationKnowledgeTribeFormation = Clan.MinSocialOrganizationValue;
    public const int MinSocialOrganizationKnowledgeValue = 200;
    public const int OptimalSocialOrganizationKnowledgeValue = 10000;

    public TribeFormationEvent()
    {
        DoNotSerialize = true;
    }

    public TribeFormationEvent(CellGroup group, long triggerDate) : base(group, triggerDate, TribeFormationEventId)
    {
        DoNotSerialize = true;
    }

    public static long CalculateTriggerDate(CellGroup group)
    {
        group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out int socialOrganizationValue);

        float randomFactor = group.Cell.GetNextLocalRandomFloat(RngOffsets.TRIBE_FORMATION_EVENT_CALCULATE_TRIGGER_DATE);
        randomFactor = Mathf.Pow(randomFactor, 2);

        float socialOrganizationFactor = (socialOrganizationValue - MinSocialOrganizationKnowledgeValue) / (OptimalSocialOrganizationKnowledgeValue - MinSocialOrganizationKnowledgeValue);
        socialOrganizationFactor = Mathf.Pow(socialOrganizationFactor, 2);
        socialOrganizationFactor = Mathf.Clamp(socialOrganizationFactor, 0.001f, 1);

        float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / socialOrganizationFactor;

        long targetDate = (long)(group.World.CurrentDate + dateSpan) + CellGroup.GenerationSpan;

        if (targetDate <= group.World.CurrentDate)
        {
            // targetDate is invalid, generate report
            Debug.LogWarning("TribeFormationEvent.CalculateTriggerDate - targetDate (" + targetDate +
                ") less or equal to World.CurrentDate (" + group.World.CurrentDate +
                "). dateSpan: " + dateSpan + ", DateSpanFactorConstant: " + DateSpanFactorConstant +
                ", socialOrganizationFactor: " + socialOrganizationFactor + ", randomFactor: " + randomFactor);

            targetDate = int.MinValue;
        }
        else if (targetDate > World.MaxSupportedDate)
        {
            // targetDate is invalid, generate report
            Debug.LogWarning("TribeFormationEvent.CalculateTriggerDate - targetDate (" + targetDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). dateSpan: " + dateSpan + ", DateSpanFactorConstant: " + DateSpanFactorConstant +
                ", socialOrganizationFactor: " + socialOrganizationFactor + ", randomFactor: " + randomFactor);

            targetDate = int.MinValue;
        }

        return targetDate;
    }

    public static bool CanSpawnIn(CellGroup group)
    {
        if (group.Population < Tribe.MinPopulationForTribeCore)
            return false;

        if (!group.HasProperty(Polity.CanFormPolityAttribute + "tribe"))
            return false;

        int value = 0;

        if (!group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out value))
            return false;

        if (value < MinSocialOrganizationKnowledgeTribeFormation)
            return false;

        return true;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;

        if (Group.Population < Tribe.MinPopulationForTribeCore)
            return false;

        if (!Group.HasProperty(Polity.CanFormPolityAttribute + "tribe"))
            return false;

        int value = 0;

        if (!Group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out value))
            return false;

        if (value < MinSocialOrganizationKnowledgeTribeFormation)
            return false;

        float prominenceFactor = Mathf.Min(1, Group.TotalPolityProminenceValue * 3f);

        if (prominenceFactor > 0)
            return false;

        return true;
    }

    public override void Trigger()
    {
        Territory encompassingTerritory = Group.Cell.EncompassingTerritory;

        Tribe tribe = new Tribe(Group);
        tribe.Initialize();

        World.AddPolityInfo(tribe);
        World.AddPolityToUpdate(tribe);

        World.AddGroupToUpdate(Group);

        PolityFormationEventMessage formationEventMessage = new PolityFormationEventMessage(tribe, TriggerDate);

        if (!World.HasEventMessage(WorldEvent.PolityFormationEventId))
        {
            World.AddEventMessage(formationEventMessage);
            formationEventMessage.First = true;
        }

        if (encompassingTerritory != null)
        {
            encompassingTerritory.Polity.AddEventMessage(formationEventMessage);
        }
    }

    protected override void DestroyInternal()
    {
        //if (Group != null)
        //{
        //    Group.UnsetFlag(EventSetFlag);
        //}

        if (Group != null)
        {
            Group.HasTribeFormationEvent = false;
        }

        base.DestroyInternal();
    }
}

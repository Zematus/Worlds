using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class SocialOrganizationKnowledge : CellCulturalKnowledge
{
    public const string SocialOrganizationKnowledgeId = "SocialOrganizationKnowledge";
    public const string SocialOrganizationKnowledgeName = "Social Organization";

    public const int SocialOrganizationKnowledgeRngOffset = 2;

    public const int InitialValue = 100;

    public const int MinValueForTribalismDiscovery = 600;
    public const int MinValueForHoldingTribalism = 200;

    public const int BaseAsymptote = 1000;
    public const int TribalismDiscoveryAsymptote = 10000;

    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;
    public const float PopulationDensityModifier = 10000f * ValueScaleFactor;

    public static int HighestAsymptote = 0;

    public SocialOrganizationKnowledge()
    {
        if (Asymptote > HighestAsymptote)
        {
            HighestAsymptote = Asymptote;
        }
    }

    public SocialOrganizationKnowledge(CellGroup group, int initialValue) : base(group, SocialOrganizationKnowledgeId, SocialOrganizationKnowledgeName, SocialOrganizationKnowledgeRngOffset, initialValue)
    {

    }

    public static bool IsSocialOrganizationKnowledge(CulturalKnowledge knowledge)
    {
        return knowledge.Id.Contains(SocialOrganizationKnowledgeId);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();
    }

    private float CalculatePopulationFactor()
    {
        float popFactor = Group.Population;

        float densityFactor = PopulationDensityModifier * Asymptote * Group.Cell.MaxAreaPercent;

        float finalPopFactor = popFactor / (popFactor + densityFactor);
        finalPopFactor = 0.1f + finalPopFactor * 0.9f;

        return finalPopFactor;
    }

    private float CalculatePolityProminenceFactor()
    {
        // This should actually depend on the type of polity, tribes should have little effect
        float totalProminence = Group.TotalPolityProminenceValue * 0.5f;

        return totalProminence;
    }

    protected override void UpdateInternal(long timeSpan)
    {
        float populationFactor = CalculatePopulationFactor();

        float prominenceFactor = Group.TotalPolityProminenceValue;

        float totalFactor = populationFactor + (prominenceFactor * (1 - populationFactor));

        UpdateValueInternal(timeSpan, TimeEffectConstant, totalFactor);

        TryGenerateTribalismDiscoveryEvent();
    }

    public override void PolityCulturalProminence(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan)
    {
        PolityCulturalProminenceInternal(polityKnowledge, polityProminence, timeSpan, TimeEffectConstant);

#if DEBUG
        if (_newValue < SocialOrganizationKnowledge.MinValueForHoldingTribalism)
        {

            if (Group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning("group with low social organization has faction cores - Id: " + Group.Id);
            }
        }
#endif

        TryGenerateTribalismDiscoveryEvent();
    }

    private void TryGenerateTribalismDiscoveryEvent()
    {
        if (Value < TribalismDiscoveryEvent.MinSocialOrganizationKnowledgeForTribalismDiscovery)
            return;

        if (Value > TribalismDiscoveryEvent.OptimalSocialOrganizationKnowledgeValue)
            return;

        if (TribalismDiscoveryEvent.CanSpawnIn(Group))
        {
            long triggerDate = TribalismDiscoveryEvent.CalculateTriggerDate(Group);

            if (triggerDate == long.MinValue)
                return;

            Group.World.InsertEventToHappen(new TribalismDiscoveryEvent(Group, triggerDate));
        }
    }

    protected override int CalculateAsymptoteInternal(CulturalDiscovery discovery)
    {
        switch (discovery.Id)
        {
            case TribalismDiscovery.TribalismDiscoveryId:
                return TribalismDiscoveryAsymptote;
        }

        return 0;
    }

    public override float CalculateExpectedProgressLevel()
    {
        float populationFactor = CalculatePopulationFactor();

        if (populationFactor <= 0)
            return 1;

        return Mathf.Clamp(ProgressLevel / populationFactor, MinProgressLevel, 1);
    }

    public override float CalculateTransferFactor()
    {
        float populationFactor = CalculatePopulationFactor();

        return (populationFactor * 0.9f) + 0.1f;
    }

    public override bool WillBeLost()
    {
        return false;
    }

    public override void LossConsequences()
    {

    }

    protected override int GetBaseAsymptote()
    {
        return BaseAsymptote;
    }
}

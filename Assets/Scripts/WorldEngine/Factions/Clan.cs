﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

// Clan Leadership:
// -- Authority factors:
// ---- Agent Charisma
// ---- Agent Wisdom
// ---- Agent Timespan as Leader * Clan's Authority

// -- Leader Authority has an effect on the chances of the tribe splitting: Greater authority = less chance of splitting
// -- Clan Cohesion has also an effect on the chances of the tribe splitting: Greater cohesion = less chance of splitting
// -- Preventing a clan from splitting will reduce the clan's respect for authority but increases the overall clan cohesion

public class Clan : Faction
{
    public const int LeadershipAvgSpan = 20 * World.YearLength;
    public const int MinClanLeaderStartAge = 16 * World.YearLength;
    public const int MaxClanLeaderStartAge = 50 * World.YearLength;

    public const int MinSocialOrganizationValue = 600;

    public const int MinCorePopulation = 500;
    public const float MinCorePolityProminence = 0.3f;

    public const float AvgClanSplitRelationshipValue = 0.5f;
    public const float ClanSplitRelationshipValueSpread = 0.1f;
    public const float ClanSplitRelationshipValueCharismaFactor = 50f;

    public const string FactionType = "Clan";
    public const string FactionNameFormat = "clan {0}";

    public Clan()
    {

    }

    public Clan(Polity polity, CellGroup coreGroup, float influence, Clan parentClan = null) : base(FactionType, polity, coreGroup, influence, parentClan)
    {

    }

    protected override void InitializeInternal()
    {
        base.InitializeInternal();

        long triggerDate = ClanCoreMigrationEvent.CalculateTriggerDate(this);
        if (triggerDate > 0)
        {
            if (triggerDate <= World.CurrentDate)
            {
                throw new System.Exception(
                    "ClanCoreMigrationEvent Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + World.CurrentDate);
            }

            AddEvent(new ClanCoreMigrationEvent(this, triggerDate));
        }

        triggerDate = ClanSplitDecisionEvent.CalculateTriggerDate(this);
        if (triggerDate > 0)
        {
            if (triggerDate <= World.CurrentDate)
            {
                throw new System.Exception(
                    "ClanSplitDecisionEvent Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + World.CurrentDate);
            }

            AddEvent(new ClanSplitDecisionEvent(this, triggerDate));
        }

        triggerDate = ClanDemandsInfluenceDecisionEvent.CalculateTriggerDate(this);
        if (triggerDate > 0)
        {
            if (triggerDate <= World.CurrentDate)
            {
                throw new System.Exception(
                    "ClanDemandsInfluenceDecisionEvent Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + World.CurrentDate);
            }

            AddEvent(new ClanDemandsInfluenceDecisionEvent(this, triggerDate));
        }

        triggerDate = TribeSplitDecisionEvent.CalculateTriggerDate(this);
        if (triggerDate > 0)
        {
            if (triggerDate <= World.CurrentDate)
            {
                throw new System.Exception(
                    "TribeSplitDecisionEvent Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + World.CurrentDate);
            }

            AddEvent(new TribeSplitDecisionEvent(this, triggerDate));
        }
    }

    public CellGroup GetCoreGroupMigrationTarget()
    {
        Direction migrationDirection = CoreGroup.GenerateCoreMigrationDirection();

        if (migrationDirection == Direction.Null)
        {
            return null;
        }

        return CoreGroup.Neighbors[migrationDirection];
    }

    protected override void GenerateEventsFromData()
    {
        foreach (FactionEventData eData in EventDataList)
        {
            switch (eData.TypeId)
            {
                case WorldEvent.ClanCoreMigrationEventId:
                    AddEvent(new ClanCoreMigrationEvent(this, eData));
                    break;
                case WorldEvent.ClanSplitDecisionEventId:
                    AddEvent(new ClanSplitDecisionEvent(this, eData));
                    break;
                case WorldEvent.TribeSplitDecisionEventId:
                    AddEvent(new TribeSplitDecisionEvent(this, eData));
                    break;
                case WorldEvent.ClanDemandsInfluenceDecisionEventId:
                    AddEvent(new ClanDemandsInfluenceDecisionEvent(this, eData));
                    break;
                default:
                    throw new System.Exception("Unhandled faction event type id: " + eData.TypeId);
            }
        }
    }

    protected override void UpdateInternal()
    {
        if (NewCoreGroup != null)
        {
            if (IsGroupValidCore(NewCoreGroup))
            {
                MigrateToNewCoreGroup();
            }

            NewCoreGroup = null;

            ResetEvent(WorldEvent.ClanCoreMigrationEventId, ClanCoreMigrationEvent.CalculateTriggerDate(this));
        }
    }

    protected override void GenerateName(Faction parentFaction)
    {
        int rngOffset = RngOffsets.CLAN_GENERATE_NAME + unchecked((int)Polity.Id);

        if (parentFaction != null)
            rngOffset += unchecked((int)parentFaction.Id);

        GetRandomIntDelegate getRandomInt = (int maxValue) => Polity.GetNextLocalRandomInt(rngOffset++, maxValue);
        GetRandomFloatDelegate getRandomFloat = () => Polity.GetNextLocalRandomFloat(rngOffset++);

        Language language = Polity.Culture.Language;
        Region region = CoreGroup.Cell.Region;

        string untranslatedName = "";

        if (region.Elements.Count <= 0)
        {
            throw new System.Exception("No elements to choose name from");
        }

        List<string> possibleAdjectives = null;

        List<Element.Instance> remainingElements = new List<Element.Instance>(region.Elements);

        bool addMoreWords = true;

        bool isPrimaryNoun = true;
        float extraWordChance = 0.2f;

        List<Element.Instance> usedElements = new List<Element.Instance>();

        while (addMoreWords)
        {
            addMoreWords = false;

            bool hasRemainingElements = remainingElements.Count > 0;

            if ((!hasRemainingElements) && (usedElements.Count <= 0))
            {
                throw new System.Exception("No elements to use for name");
            }

            Element.Instance element = null;

            if (hasRemainingElements)
            {
                element = remainingElements.RandomSelectAndRemove(getRandomInt);

                usedElements.Add(element);
            }
            else
            {
                element = usedElements.RandomSelect(getRandomInt);
            }

            if (isPrimaryNoun)
            {
                untranslatedName = element.SingularName;
                isPrimaryNoun = false;

                possibleAdjectives = element.Adjectives;
            }
            else
            {
                bool first = true;
                foreach (Element.Instance usedElement in usedElements)
                {
                    if (first)
                    {
                        untranslatedName = usedElement.SingularName;
                        first = false;
                    }
                    else
                    {
                        untranslatedName = usedElement.SingularName + ":" + untranslatedName;
                    }
                }
            }

            string adjective = possibleAdjectives.RandomSelect(getRandomInt, 2 * usedElements.Count);

            if (!string.IsNullOrEmpty(adjective))
            {
                untranslatedName = "[adj]" + adjective + " " + untranslatedName;
            }

            addMoreWords = extraWordChance > getRandomFloat();

            if (!addMoreWords)
            {
                foreach (Faction faction in Polity.GetFactions())
                {
                    if (Language.ClearConstructCharacters(untranslatedName) == faction.Name.Meaning)
                    {
                        addMoreWords = true;
                        break;
                    }
                }
            }

            extraWordChance /= 2f;
        }

        untranslatedName = "[Proper][NP](" + untranslatedName + ")";

        Info.Name = new Name(untranslatedName, language, World);

        //		#if DEBUG
        //		Debug.Log ("Clan #" + Id + " name: " + Name);
        //		#endif
    }

    protected override Agent RequestCurrentLeader()
    {
        return RequestCurrentLeader(LeadershipAvgSpan, MinClanLeaderStartAge, MaxClanLeaderStartAge, RngOffsets.CLAN_LEADER_GEN_OFFSET);
    }

    protected override Agent RequestNewLeader()
    {
        return RequestNewLeader(LeadershipAvgSpan, MinClanLeaderStartAge, MaxClanLeaderStartAge, RngOffsets.CLAN_LEADER_GEN_OFFSET);
    }

    public static bool CanBeClanCore(CellGroup group)
    {
        if (!group.HasProperty(Polity.CanFormPolityAttribute + "tribe"))
        {
            return false;
        }

        int value = 0;

        if (!group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out value))
        {
            return false;
        }

        bool hasMinSocialOrg = value >= MinSocialOrganizationValue;

        return hasMinSocialOrg;
    }

    public bool IsGroupValidCore(CellGroup group)
    {
        if (!CanBeClanCore(group))
            return false;

        if (!group.HasProperty(Polity.CanFormPolityAttribute + "tribe"))
            return false;

        PolityProminence pi = group.GetPolityProminence(Polity);

        if (pi == null)
            return false;

        if (pi.Value < MinCorePolityProminence)
            return false;

        if (group.Population < MinCorePopulation)
            return false;

        return true;
    }

    public override bool ShouldMigrateFactionCore(CellGroup sourceGroup, CellGroup targetGroup)
    {
        if (!CanBeClanCore(targetGroup))
            return false;

        PolityProminence piTarget = targetGroup.GetPolityProminence(Polity);

        if (piTarget != null)
        {
            int targetGroupPopulation = targetGroup.Population;
            float targetGroupProminence = piTarget.Value;

            return ShouldMigrateFactionCore(sourceGroup, targetGroup.Cell, targetGroupProminence, targetGroupPopulation);
        }

        return false;
    }

    public override bool ShouldMigrateFactionCore(CellGroup sourceGroup, TerrainCell targetCell, float targetProminence, int targetPopulation)
    {
//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (sourceGroup.Id == Manager.TracingData.GroupId)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "ShouldMigrateFactionCore - Clan:" + Id + ", sourceGroup:" + sourceGroup.Id,
//                    "CurrentDate: " + World.CurrentDate +
//                    ", targetPopulation: " + targetPopulation +
//                    ", targetProminence: " + targetProminence +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        float targetProminenceFactor = Mathf.Max(0, targetProminence - MinCorePolityProminence);

        if (targetProminenceFactor <= 0)
            return false;

        float targetPopulationFactor = Mathf.Max(0, targetPopulation - MinCorePopulation);

        if (targetPopulationFactor <= 0)
            return false;

        int sourcePopulation = sourceGroup.Population;

        PolityProminence pi = sourceGroup.GetPolityProminence(Polity);

        if (pi == null)
        {
            Debug.LogError("Unable to find Polity with Id: " + Polity.Id);
        }

        float sourceProminence = pi.Value;

        float sourceProminenceFactor = Mathf.Max(0, sourceProminence - MinCorePolityProminence);
        float sourcePopulationFactor = Mathf.Max(0, sourcePopulation - MinCorePopulation);

        float sourceFactor = sourceProminenceFactor * sourcePopulationFactor;

//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (sourceGroup.Id == Manager.TracingData.GroupId)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "ShouldMigrateFactionCore - Clan:" + Id + ", sourceGroup:" + sourceGroup.Id,
//                    "CurrentDate: " + World.CurrentDate +
//                    ", sourceProminence: " + sourceProminence +
//                    ", sourcePopulation: " + sourcePopulation +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        if (sourceFactor <= 0)
            return true;

        float targetFactor = targetProminenceFactor * targetPopulationFactor;

        float migrateCoreFactor = sourceFactor / (sourceFactor + targetFactor);

        float randomValue = sourceGroup.GetNextLocalRandomFloat(RngOffsets.MIGRATING_GROUP_MOVE_FACTION_CORE + unchecked((int)Id));

//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (sourceGroup.Id == Manager.TracingData.GroupId)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "ShouldMigrateFactionCore - Clan:" + Id + ", sourceGroup:" + sourceGroup.Id,
//                    "CurrentDate: " + World.CurrentDate +
//                    ", randomValue: " + randomValue +
//                    ", migrateCoreFactor: " + migrateCoreFactor +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        return (randomValue > migrateCoreFactor);
    }

    public override void Split()
    {
        int randomOffset = unchecked((int)(RngOffsets.CLAN_SPLIT + Id));

        float randomValue = GetNextLocalRandomFloat(randomOffset++);
        float splitFactionInfluence = _splitFactionMinInfluence + (randomValue * (_splitFactionMaxInfluence - _splitFactionMinInfluence));

        Influence -= splitFactionInfluence;

        if (_splitFactionCoreGroup == null)
        {
            throw new System.Exception("_splitFactionCoreGroup is null - Clan Id: " + Id + ", Event Id: " + _splitFactionEventId);
        }

        float polityProminenceValue = _splitFactionCoreGroup.GetPolityProminenceValue(Polity);
        PolityProminence highestPolityProminence = _splitFactionCoreGroup.HighestPolityProminence;

        if (highestPolityProminence == null)
        {
            throw new System.Exception(
                "highestPolityProminence is null - Clan Id: " + Id +
                ", Group Id: " + _splitFactionCoreGroup.Id +
                ", Event Id: " + _splitFactionEventId);
        }

        if (CurrentLeader == null)
        {
            throw new System.Exception("CurrentLeader is null - Clan Id: " + Id + ", Event Id: " + _splitFactionEventId);
        }

        Tribe parentTribe = Polity as Tribe;

        if (parentTribe == null)
        {
            throw new System.Exception("parentTribe is null - Clan Id: " + Id + ", Event Id: " + _splitFactionEventId);
        }

        // If the polity with the highest prominence is different than the source clan's polity and it's value is twice greater switch the new clan's polity to this one.
        // NOTE: This is sort of a hack to avoid issues with clan/tribe split coincidences (issue #8 github). Try finding a better solution...
        if (highestPolityProminence.Value > (polityProminenceValue * 2))
        {

            if (highestPolityProminence.Polity is Tribe)
            {
                parentTribe = highestPolityProminence.Polity as Tribe;

                //#if DEBUG
                //                Debug.Log("parent tribe replaced from " + Polity.Id + " to " + parentTribe.Id + " due to low polity prominence value in new core: " + polityProminenceValue);
                //#endif
            }
            else
            {
                throw new System.Exception("Failed to replace new parent polity as it is not a Tribe. Id: " + highestPolityProminence.Polity.Id + ", Event Id: " + _splitFactionEventId);
            }
        }

        Clan newClan = new Clan(parentTribe, _splitFactionCoreGroup, splitFactionInfluence, this);

        if (newClan == null)
        {
            throw new System.Exception("newClan is null - Clan Id: " + Id + ", Event Id: " + _splitFactionEventId);
        }

        newClan.Initialize(); // We can initialize right away since the containing polity is already initialized

        // set relationship with parent clan

        float parentClanRelationshipValue = AvgClanSplitRelationshipValue + (CurrentLeader.Charisma - 10) / ClanSplitRelationshipValueCharismaFactor;

        randomValue = GetNextLocalRandomFloat(randomOffset++);
        float relationshipValue = parentClanRelationshipValue + (ClanSplitRelationshipValueSpread * (2f * randomValue - 1f));

        SetRelationship(this, newClan, relationshipValue);

        parentTribe.AddFaction(newClan);

        parentTribe.UpdateDominantFaction();

        World.AddFactionToUpdate(this);
        World.AddFactionToUpdate(newClan);
        World.AddPolityToUpdate(Polity);

        parentTribe.AddEventMessage(new ClanSplitEventMessage(this, newClan, World.CurrentDate));
    }

    public float CalculateAdministrativeLoad()
    {
        int socialOrganizationValue = 0;

        Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out socialOrganizationValue);
        
        if (socialOrganizationValue <= 0)
        {
            return Mathf.Infinity;
        }

        float administrativeLoad = Polity.TotalAdministrativeCost * Influence / (float)socialOrganizationValue;

        administrativeLoad = Mathf.Pow(administrativeLoad, 2);

        if (administrativeLoad < 0)
        {
            Debug.LogWarning("administrativeLoad less than 0: " + administrativeLoad + ", Clan Id: " + Id);

            return Mathf.Infinity;
        }

//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (Manager.TracingData.FactionId == Id)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "Clan:CalculateAdministrativeLoad - ClanId:" + Id,
//                    "CurrentDate: " + World.CurrentDate +
//                    ", socialOrganizationValue: " + socialOrganizationValue +
//                    ", Influence: " + Influence +
//                    ", Polity.TotalAdministrativeCost: " + Polity.TotalAdministrativeCost +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        return administrativeLoad;
    }
}

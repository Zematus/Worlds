﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

public class Tribe : Polity
{
    public const int MinPopulationForTribeCore = 500;

    public const float TribalExpansionFactor = 2f;

    public const int TribeLeaderAvgTimeSpan = 41;

    public const string PolityTypeStr = "tribe";
    public const string PolityNameFormat = "the {0} tribe";

    public const float BaseCoreProminence = 0.5f;

    private static string[] PrepositionVariations = new string[] { "from", "of" };

    private static Variation[] TribeNounVariations;

    private static string[] TribeNounVariants = new string[] {
        "nation", "tribe", "[in(person)]people", "folk", "community", "kin", "{kin:s:}person:s", "{kin:s:}[in(man)]men", "{kin:s:}[in(woman)]women", "[in(child)]children" };
    
    private int _rngOffset;

    public Tribe()
    {

    }

    public Tribe(CellGroup coreGroup) : base(PolityTypeStr, coreGroup)
    {
        //// Make sure there's a region to spawn into

        TerrainCell coreCell = coreGroup.Cell;

        Region cellRegion = coreGroup.Cell.Region;

        if (cellRegion == null)
        {
            //			#if DEBUG
            //			if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
            //				if ((Id == Manager.TracingData.PolityId) && (coreCell.Longitude == Manager.TracingData.Longitude) && (coreCell.Latitude == Manager.TracingData.Latitude)) {
            //					bool debug = true;
            //				}
            //			}
            //			#endif

            cellRegion = Region.TryGenerateRegion(coreCell, Culture.Language);

            if (cellRegion != null)
            {
                if (World.GetRegionInfo(cellRegion.Id) != null)
                {
                    throw new System.Exception("RegionInfo with Id " + cellRegion.Id + " already present");
                }

                World.AddRegionInfo(cellRegion.Info);
            }
            else
            {
                throw new System.Exception("No region could be generated");
            }
        }

        ////

        float randomValue = coreGroup.Cell.GetNextLocalRandomFloat(RngOffsets.TRIBE_GENERATE_NEW_TRIBE);
        float coreProminence = BaseCoreProminence + randomValue * (1 - BaseCoreProminence);

        coreGroup.SetPolityProminence(this, coreProminence, 0, 0);

        GenerateName();

        //		Debug.Log ("New tribe '" + Name + "' spawned at " + coreGroup.Cell.Position);

        //// Add starting clan

        Clan clan = new Clan(this, coreGroup, 1); // Clan should be initialized when the Tribe gets initialized

        AddFaction(clan);

        SetDominantFaction(clan);
    }

    public Tribe(Clan triggerClan, Polity parentPolity) : base(PolityTypeStr, triggerClan.CoreGroup, triggerClan.Id)
    {
        triggerClan.ChangePolity(this, triggerClan.Influence);

        SwitchCellProminences(parentPolity, triggerClan);

        GenerateName();

        //		Debug.Log ("New tribe '" + Name + "' from tribe '" + parentPolity.Name + "' with total transfered influence = " + transferedInfluence);
    }

    public override void InitializeInternal()
    {
        long triggerDate = FosterTribeRelationDecisionEvent.CalculateTriggerDate(this);
        if (triggerDate > 0)
        {
            if (triggerDate <= World.CurrentDate)
            {
                throw new System.Exception(
                    "FosterTribeRelationDecisionEvent Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + World.CurrentDate);
            }

            AddEvent(new FosterTribeRelationDecisionEvent(this, triggerDate));
        }

        triggerDate = MergeTribesDecisionEvent.CalculateTriggerDate(this);
        if (triggerDate > 0)
        {
            if (triggerDate <= World.CurrentDate)
            {
                throw new System.Exception(
                    "MergeTribesDecisionEvent Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + World.CurrentDate);
            }

            AddEvent(new MergeTribesDecisionEvent(this, triggerDate));
        }

        triggerDate = OpenTribeDecisionEvent.CalculateTriggerDate(this);
        if (triggerDate > 0)
        {
            if (triggerDate <= World.CurrentDate)
            {
                throw new System.Exception(
                    "OpenTribeDecisionEvent Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + World.CurrentDate);
            }

            AddEvent(new OpenTribeDecisionEvent(this, triggerDate));
        }
    }

    public static void GenerateTribeNounVariations()
    {
        TribeNounVariations = NameTools.GenerateNounVariations(TribeNounVariants);
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

    private void SwitchCellProminences(Polity sourcePolity, Clan triggerClan)
    {
        float targetPolityInfluence = triggerClan.Influence;
        float sourcePolityInfluence = 1 - targetPolityInfluence;
        
        if (targetPolityInfluence <= 0)
        {
            throw new System.Exception("Pulling clan influence equal or less than zero.");
        }
        
        int maxGroupCount = sourcePolity.Groups.Count;

        Dictionary<CellGroup, float> groupDistances = new Dictionary<CellGroup, float>(maxGroupCount);

        Queue<CellGroup> sourceGroups = new Queue<CellGroup>(maxGroupCount);

        sourceGroups.Enqueue(CoreGroup);

        int reviewedCells = 0;
        int switchedCells = 0;

        HashSet<Faction> factionsToTransfer = new HashSet<Faction>();

        while (sourceGroups.Count > 0)
        {
            CellGroup group = sourceGroups.Dequeue();

            if (groupDistances.ContainsKey(group))
                continue;

            PolityProminence pi = group.GetPolityProminence(sourcePolity);

            if (pi == null)
                continue;

            reviewedCells++;

            float distanceToTargetPolityCore = CalculateShortestCoreDistance(group, groupDistances);

            if (distanceToTargetPolityCore >= CellGroup.MaxCoreDistance)
                continue;

            groupDistances.Add(group, distanceToTargetPolityCore);

            float distanceToSourcePolityCore = pi.PolityCoreDistance;

            float percentProminence = 1f;

            if (distanceToSourcePolityCore < CellGroup.MaxCoreDistance)
            {
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

            if (percentProminence > 0.5f)
            {
                switchedCells++;

                foreach (Faction faction in group.GetFactionCores())
                {
                    // Do not transfer factions that belong to polities other than the source one
                    if (faction.Polity != sourcePolity)
                        continue;

                    if (sourcePolity.DominantFaction == faction)
                    {
                        new System.Exception("Dominant Faction getting switched...");
                    }

                    //					#if DEBUG
                    //					if (sourcePolity.FactionCount == 1) {
                    //						throw new System.Exception ("Number of factions in Polity " + Id + " will be equal or less than zero. Current Date: " + World.CurrentDate);
                    //					}
                    //					#endif

                    factionsToTransfer.Add(faction);
                }
            }

            float prominenceValue = pi.Value;

            group.SetPolityProminence(sourcePolity, prominenceValue * (1 - percentProminence));

            group.SetPolityProminence(this, prominenceValue * percentProminence, distanceToTargetPolityCore, distanceToTargetPolityCore);

            World.AddGroupToUpdate(group);

            foreach (CellGroup neighborGroup in group.NeighborGroups)
            {
                if (groupDistances.ContainsKey(neighborGroup))
                    continue;

                sourceGroups.Enqueue(neighborGroup);
            }
        }

        float highestInfluence = triggerClan.Influence;
        Clan dominantClan = triggerClan;

        foreach (Faction faction in factionsToTransfer)
        {
            if (faction is Clan clan)
            {
                if (clan.Influence > highestInfluence)
                {
                    highestInfluence = clan.Influence;
                    dominantClan = clan;
                }
            }

            faction.ChangePolity(this, faction.Influence);
        }

        SetDominantFaction(dominantClan);

        //		Debug.Log ("SwitchCellProminences: source polity cells: " + maxGroupCount + ", reviewed cells: " + reviewedCells + ", switched cells: " + switchedCells);
    }

    private float CalculateShortestCoreDistance(CellGroup group, Dictionary<CellGroup, float> groupDistances)
    {
        if (groupDistances.Count <= 0)
            return 0;

        float shortestDistance = CellGroup.MaxCoreDistance;

        foreach (KeyValuePair<Direction, CellGroup> pair in group.Neighbors)
        {
            float distanceToCoreFromNeighbor = float.MaxValue;

            if (!groupDistances.TryGetValue(pair.Value, out distanceToCoreFromNeighbor))
            {
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

    protected override void UpdateInternal()
    {
    }

    private int GetRandomInt(int maxValue)
    {
        return GetNextLocalRandomInt(_rngOffset++, maxValue);
    }

    private float GetRandomFloat()
    {
        return GetNextLocalRandomFloat(_rngOffset++);
    }

    protected override void GenerateName()
    {
        Region coreRegion = CoreGroup.Cell.Region;

        _rngOffset = RngOffsets.TRIBE_GENERATE_NAME + unchecked((int)Id);

        string tribeNoun = TribeNounVariations.RandomSelect(GetRandomInt).Text;

        bool areaNameIsNounAdjunct = (GetRandomFloat() > 0.5f);

        string areaName = coreRegion.GetRandomUnstranslatedAreaName(GetRandomInt, areaNameIsNounAdjunct);

        string untranslatedName;

        if (areaNameIsNounAdjunct)
        {
            untranslatedName = "[Proper][NP](" + areaName + " " + tribeNoun + ")";
        }
        else
        {
            string preposition = PrepositionVariations.RandomSelect(GetRandomInt);

            untranslatedName = "[PpPP]([Proper][NP](" + tribeNoun + ") [PP](" + preposition + " [Proper][NP](the " + areaName + ")))";
        }

        Info.Name = new Name(untranslatedName, Culture.Language, World);

//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            //if (Manager.TracingData.PolityId == Id)
//            //{
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "Tribe.GenerateName - Polity.Id:" + Id,
//                    "CurrentDate: " + World.CurrentDate +
//                    ", PrepositionVariations.Length: " + PrepositionVariations.Length +
//                    //", PrepositionVariations: [" + string.Join(",", PrepositionVariations) + "]" +
//                    ", PrepositionVariations.Length: " + PrepositionVariations.Length +
//                    ", TribeNounVariations.Length: " + TribeNounVariations.Length +
//                    ", areaName: " + areaName +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            //}
//        }
//#endif

        //		#if DEBUG
        //		Debug.Log ("Tribe #" + Id + " name: " + Name);
        //		#endif
    }

    public override float CalculateGroupProminenceExpansionValue(CellGroup sourceGroup, CellGroup targetGroup, float sourceValue)
    {
        if (sourceValue <= 0)
            return 0;

        float sourceGroupTotalPolityProminenceValue = sourceGroup.TotalPolityProminenceValue;
        float targetGroupTotalPolityProminenceValue = targetGroup.TotalPolityProminenceValue;

        if (sourceGroupTotalPolityProminenceValue <= 0)
        {
            throw new System.Exception("sourceGroup.TotalPolityProminenceValue equal or less than 0: " + sourceGroupTotalPolityProminenceValue);
        }

        float prominenceFactor = sourceGroupTotalPolityProminenceValue / (targetGroupTotalPolityProminenceValue + sourceGroupTotalPolityProminenceValue);
        prominenceFactor = Mathf.Pow(prominenceFactor, 4);

        float modifiedForagingCapacity = 0;
        float modifiedSurvivability = 0;

        CalculateAdaptionToCell(targetGroup.Cell, out modifiedForagingCapacity, out modifiedSurvivability);

        float survivabilityFactor = Mathf.Pow(modifiedSurvivability, 2);

        float finalFactor = prominenceFactor * survivabilityFactor;

        if (sourceGroup != targetGroup)
        {
            // There should be a strong bias against polity expansion to reduce activity
            finalFactor *= TribalExpansionFactor;
        }

        return finalFactor;
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();
    }
}

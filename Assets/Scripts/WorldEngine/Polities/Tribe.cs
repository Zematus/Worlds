using UnityEngine;
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
        float randomValue = coreGroup.Cell.GetNextLocalRandomFloat(RngOffsets.TRIBE_GENERATE_NEW_TRIBE);
        float coreProminenceFactor = BaseCoreProminence + randomValue * (1 - BaseCoreProminence);

        // the initial prominence can only be taken from the unorganized bands in the core group
        float ubProminence = 1f - coreGroup.TotalPolityProminenceValue;
        coreProminenceFactor *= ubProminence;

        if (coreProminenceFactor == 0)
        {
            throw new System.Exception(
                "Unable to assign a core prominence bigger than zero. Group: " + coreGroup);
        }

        Clan clan = new Clan(this, coreGroup, 1);
        // Clan should be initialized when the Tribe gets initialized

        AddFaction(clan);

        SetDominantFaction(clan, false);

        coreGroup.AddPolityProminence(this, coreProminenceFactor, true);
        coreGroup.FindHighestPolityProminence();

        GenerateName();
    }

    public Tribe(Clan triggerClan, Polity parentPolity) :
        base(PolityTypeStr, triggerClan.CoreGroup, triggerClan.GetHashCode())
    {
        triggerClan.ChangePolity(this, triggerClan.Influence);

        SwitchCellProminences(parentPolity, triggerClan);

        GenerateName();

        //		Debug.Log ("New tribe '" + Name + "' from tribe '" + parentPolity.Name + "' with total transfered influence = " + transferedInfluence);
    }

    public static void GenerateTribeNounVariations()
    {
        TribeNounVariations = NameTools.GenerateNounVariations(TribeNounVariants);
    }

    private void SwitchCellProminences(Polity sourcePolity, Clan triggerClan)
    {
//#if DEBUG
//        Manager.DebugPauseSimRequested = true;
//#endif

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
        Dictionary<CellGroup, float> groupsToTransfer = new Dictionary<CellGroup, float>();

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

            if (distanceToTargetPolityCore >= PolityProminence.MaxCoreDistance)
                continue;

            groupDistances.Add(group, distanceToTargetPolityCore);

            float distanceToSourcePolityCore = pi.PolityCoreDistance;

            float percentProminence = 1f;

            if (distanceToSourcePolityCore < PolityProminence.MaxCoreDistance)
            {
                float ditanceToCoresSum = distanceToTargetPolityCore + distanceToSourcePolityCore;

                float distanceFactor = distanceToSourcePolityCore / ditanceToCoresSum;

                distanceFactor = Mathf.Clamp01((distanceFactor * 3f) - 1f);

                float targetDistanceFactor = distanceFactor;
                float sourceDistanceFactor = 1 - distanceFactor;

                float targetPolityWeight = targetPolityInfluence * targetDistanceFactor;
                float sourcePolityWeight = sourcePolityInfluence * sourceDistanceFactor;

                percentProminence = targetPolityWeight / (targetPolityWeight + sourcePolityWeight);

                if (float.IsNaN(percentProminence))
                {
                    throw new System.Exception("percent prominence is Nan. Tribe: " + Id);
                }
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

                    factionsToTransfer.Add(faction);
                }
            }

            float prominenceValue = pi.Value;

            float sourceProminenceValueDelta = prominenceValue * percentProminence;

            groupsToTransfer.Add(group, sourceProminenceValueDelta);

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

        SetDominantFaction(dominantClan, false);

        foreach (var pair in groupsToTransfer)
        {
            CellGroup group = pair.Key;
            float value = pair.Value;

            group.ModifyPolityProminenceValue(sourcePolity, -value);
            group.AddPolityProminence(this, value);
            group.FindHighestPolityProminence();

            World.AddGroupToUpdate(group);
        }
    }

    private float CalculateShortestCoreDistance(
        CellGroup group, Dictionary<CellGroup, float> groupDistances)
    {
        if (groupDistances.Count <= 0)
            return 0;

        float shortestDistance = PolityProminence.MaxCoreDistance;

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

        _rngOffset = RngOffsets.TRIBE_GENERATE_NAME + unchecked(GetHashCode());

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

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();
    }
}

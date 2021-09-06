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

    public Tribe(Clan triggerClan) :
        base(PolityTypeStr, triggerClan.CoreGroup, triggerClan.GetHashCode())
    {
        triggerClan.ChangePolity(this, triggerClan.Influence);
        SetDominantFaction(triggerClan, false);

        GenerateName();
    }

    public static void GenerateTribeNounVariations()
    {
        TribeNounVariations = NameTools.GenerateNounVariations(TribeNounVariants);
    }

    private Dictionary<CellGroup, float> FindGroupsToTransfer(Polity sourcePolity, Faction triggerFaction)
    {
        int maxGroupCount = sourcePolity.Groups.Count;

        HashSet<CellGroup> exploredGroups = new HashSet<CellGroup>();
        Queue<CellGroup> sourceGroups = new Queue<CellGroup>(maxGroupCount);

        sourceGroups.Enqueue(CoreGroup);
        exploredGroups.Add(CoreGroup);

        var groupsToTransfer = new Dictionary<CellGroup, float>();

        while (sourceGroups.Count > 0)
        {
            CellGroup group = sourceGroups.Dequeue();

            PolityProminence pi = group.GetPolityProminence(sourcePolity);

            if (pi == null)
                continue;

            if (pi.ClosestFaction != triggerFaction)
                continue;

            float percentProminence = 1f;

            float prominenceValue = pi.Value;

            float sourceProminenceValueDelta = prominenceValue * percentProminence;

            groupsToTransfer.Add(group, sourceProminenceValueDelta);

            foreach (CellGroup neighborGroup in group.NeighborGroups)
            {
                if (exploredGroups.Contains(neighborGroup))
                    continue;

                sourceGroups.Enqueue(neighborGroup);
                exploredGroups.Add(neighborGroup);
            }
        }

        return groupsToTransfer;
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

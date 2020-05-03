using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[System.Obsolete]
public class ClanSplitDecision : FactionDecision
{
    public const float BaseMinPreferencePercentChange = 0.15f;
    public const float BaseMaxPreferencePercentChange = 0.30f;

    public const float SplitClanMinInfluence = 0.25f;
    public const float SplitClanMaxInfluence = 0.50f;

    private Clan _clan;

    private bool _cantPrevent = false;
    private bool _preferSplit = true;

    private CellGroup _newCoreGroup;

    private static string GenerateDescriptionIntro(Clan clan)
    {
        return "Several minor bands within clan " + clan.Name.BoldText + " have become too distant, hardly interacting with the rest of the clan. Now they are becoming their own clan.\n\n";
    }

    public ClanSplitDecision(Clan clan, CellGroup newCoreGroup, long eventId) : base(clan, eventId)
    {
        _clan = clan;

        Description = GenerateDescriptionIntro(clan) +
            "Unfortunately, " + clan.CurrentLeader.Name.BoldText + " can't do anything about it under the current circumstances...";

        _cantPrevent = true;

        _newCoreGroup = newCoreGroup;
    }

    public ClanSplitDecision(Clan clan, CellGroup newCoreGroup, bool preferSplit, long eventId) : base(clan, eventId)
    {
        _clan = clan;

        Description = GenerateDescriptionIntro(clan) +
            "Should the clan leader, " + clan.CurrentLeader.Name.BoldText + ", try to reach out to them to keep them from splitting into their own clan?";

        _preferSplit = preferSplit;

        _newCoreGroup = newCoreGroup;
    }

    private string GeneratePreventSplitResultMessage()
    {
        float charismaFactor = _clan.CurrentLeader.Charisma / 10f;
        float wisdomFactor = _clan.CurrentLeader.Wisdom / 15f;

        float attributesFactor = Mathf.Max(charismaFactor, wisdomFactor);
        attributesFactor = Mathf.Clamp(attributesFactor, 0.5f, 2f);

        float minPreferencePercentChange = BaseMinPreferencePercentChange / attributesFactor;
        float maxPreferencePercentChange = BaseMaxPreferencePercentChange / attributesFactor;

        float prefValue = _clan.GetPreferenceValue(CulturalPreference.AuthorityPreferenceId);

        float minPrefChange = MathUtility.DecreaseByPercent(prefValue, minPreferencePercentChange);
        float maxPrefChange = MathUtility.DecreaseByPercent(prefValue, maxPreferencePercentChange);

        string authorityPreferenceChangeStr = "\t• Clan " + _clan.Name.BoldText + ": authority preference (" + prefValue.ToString("0.00")
            + ") decreases to: " + minPrefChange.ToString("0.00") + " - " + maxPrefChange.ToString("0.00");

        minPreferencePercentChange = BaseMinPreferencePercentChange * attributesFactor;
        maxPreferencePercentChange = BaseMaxPreferencePercentChange * attributesFactor;

        prefValue = _clan.GetPreferenceValue(CulturalPreference.CohesionPreferenceId);

        minPrefChange = MathUtility.IncreaseByPercent(prefValue, minPreferencePercentChange);
        maxPrefChange = MathUtility.IncreaseByPercent(prefValue, maxPreferencePercentChange);

        string cohesionPreferenceChangeStr = "\t• Clan " + _clan.Name.BoldText + ": cohesion preference (" + prefValue.ToString("0.00")
            + ") increases to: " + minPrefChange.ToString("0.00") + " - " + maxPrefChange.ToString("0.00");

        return authorityPreferenceChangeStr + "\n" + cohesionPreferenceChangeStr;
    }

    public static void LeaderPreventsSplit(Clan clan)
    {
        float charismaFactor = clan.CurrentLeader.Charisma / 10f;
        float wisdomFactor = clan.CurrentLeader.Wisdom / 15f;

        float attributesFactor = Mathf.Max(charismaFactor, wisdomFactor);
        attributesFactor = Mathf.Clamp(attributesFactor, 0.5f, 2f);

        int rngOffset = RngOffsets.CLAN_SPLITTING_EVENT_LEADER_PREVENTS_MODIFY_ATTRIBUTE;

        float randomFactor = clan.GetNextLocalRandomFloat(rngOffset++);
        float authorityPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
        authorityPreferencePercentChange /= attributesFactor;

        randomFactor = clan.GetNextLocalRandomFloat(rngOffset++);
        float cohesionPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
        cohesionPreferencePercentChange *= attributesFactor;

        clan.DecreasePreferenceValue(CulturalPreference.AuthorityPreferenceId, authorityPreferencePercentChange);
        clan.IncreasePreferenceValue(CulturalPreference.CohesionPreferenceId, cohesionPreferencePercentChange);

        clan.SetToUpdate();

        // Should reduce respect for authority and increase cohesion
        clan.Polity.AddEventMessage(new PreventClanSplitEventMessage(clan, clan.CurrentLeader, clan.World.CurrentDate));
    }

    private void PreventSplit()
    {
        LeaderPreventsSplit(_clan);
    }

    private string GenerateAllowSplitResultMessage()
    {
        float minInfluence;
        float maxInfluence;

        CalculateMinMaxInfluence(_clan, out minInfluence, out maxInfluence);

        string message;

        float clanInfluence = _clan.Influence;
        float minNewClanInfluence = clanInfluence - minInfluence;
        float maxNewClanInfluence = clanInfluence - maxInfluence;

        message = "\t• Clan " + _clan.Name.BoldText + ": influence (" + clanInfluence.ToString("P")
            + ") decreases to " + minNewClanInfluence.ToString("P") + " - " + maxNewClanInfluence.ToString("P");
        message += "\n\t• A new clan with influence " + minInfluence.ToString("P") + " - " + maxInfluence.ToString("P") + " splits from " + _clan.Name.BoldText;

        return message;
    }

    private void AllowSplit()
    {
        LeaderAllowsSplit(_clan, _newCoreGroup, _eventId);
    }

    public override Option[] GetOptions()
    {
        if (_cantPrevent)
        {
            return new Option[] {
                new Option ("Oh well...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
            };
        }

        return new Option[] {
            new Option ("Allow clan to split in two...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
            new Option ("Prevent clan from splitting...", "Effects:\n" + GeneratePreventSplitResultMessage (), PreventSplit)
        };
    }

    public override void ExecutePreferredOption()
    {
        if (_preferSplit)
            AllowSplit();
        else
            PreventSplit();
    }

    public static void LeaderAllowsSplit(Clan clan, CellGroup newClanCoreGroup, long eventId)
    {
        CalculateMinMaxInfluence(clan, out float minInfluence, out float maxInfluence);

        newClanCoreGroup.SetToUpdate();

        clan.SetToSplit(newClanCoreGroup, minInfluence, maxInfluence, eventId);
    }

    public static void CalculateMinMaxInfluence(Clan clan, out float minInfluence, out float maxInfluence)
    {
        float charismaFactor = clan.CurrentLeader.Charisma / 10f;
        float cultureModifier = 1 + (charismaFactor * clan.GetPreferenceValue(CulturalPreference.CohesionPreferenceId));

        minInfluence = clan.Influence * SplitClanMinInfluence / cultureModifier;
        maxInfluence = clan.Influence * SplitClanMaxInfluence / cultureModifier;
    }
}

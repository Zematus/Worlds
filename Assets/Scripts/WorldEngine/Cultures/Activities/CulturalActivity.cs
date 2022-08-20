using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(CellCulturalActivity))]
public class CulturalActivity : CulturalActivityInfo
{
    public static HashSet<string> ValidActivityIds;

    public const string ForagingActivityId = "foraging";
    public const string FarmingActivityId = "farming";
    public const string FishingActivityId = "fishing";

    public const string ForagingActivityName = "foraging";
    public const string FarmingActivityName = "farming";
    public const string FishingActivityName = "fishing";

    public const int ForagingActivityRngOffset = 0;
    public const int FarmingActivityRngOffset = 100;
    public const int FishingActivityRngOffset = 200;

    [XmlAttribute("V")]
    public float Value;

    [XmlAttribute("C")]
    public float Contribution = 0;

    public CulturalActivity()
    {
    }

    public CulturalActivity(string id, string name, int rngOffset, float value, float contribution) : base(id, name, rngOffset)
    {
        Value = value;
        Contribution = contribution;
    }

    public CulturalActivity(CulturalActivity baseActivity) : base(baseActivity)
    {
        Value = baseActivity.Value;
        Contribution = baseActivity.Contribution;
    }

    public void Reset()
    {
        Value = 0;
        Contribution = 0;
    }

    public static void ResetActivities()
    {
        ValidActivityIds = new HashSet<string>();
    }

    public static void InitializeActivities()
    {
        ValidActivityIds.Add(ForagingActivityId);
        ValidActivityIds.Add(FarmingActivityId);
        ValidActivityIds.Add(FishingActivityId);
    }
}

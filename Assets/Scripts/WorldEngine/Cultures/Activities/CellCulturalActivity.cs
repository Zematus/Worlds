using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellCulturalActivity : CulturalActivity
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

    public const string ForagingActivityId = "foraging";
    public const string FarmingActivityId = "farming";
    public const string FishingActivityId = "fishing";

    public const string ForagingActivityName = "foraging";
    public const string FarmingActivityName = "farming";
    public const string FishingActivityName = "fishing";

    public const int ForagingActivityRngOffset = 0;
    public const int FarmingActivityRngOffset = 100;
    public const int FishingActivityRngOffset = 200;

    [XmlIgnore]
    public CellGroup Group;

    private const float MaxChangeDelta = 0.2f;

    private float _newValue;

    public CellCulturalActivity()
    {
    }

    private CellCulturalActivity(CellGroup group, string id, string name, int rngOffset, float value = 0, float contribution = 0) : base(id, name, rngOffset, value, contribution)
    {
        Group = group;

        _newValue = value;
    }

    public static CellCulturalActivity CreateCellInstance(CellGroup group, CulturalActivity baseActivity)
    {
        return CreateCellInstance(group, baseActivity, baseActivity.Value);
    }

    public static CellCulturalActivity CreateCellInstance(CellGroup group, CulturalActivity baseActivity, float initialValue, float initialContribution = 0)
    {
        return new CellCulturalActivity(group, baseActivity.Id, baseActivity.Name, baseActivity.RngOffset, initialValue, initialContribution);
    }

    public static CellCulturalActivity CreateActivity(string id, CellGroup group, float value = 0, float contribution = 0)
    {
        switch (id)
        {
            case FarmingActivityId:
                return new CellCulturalActivity(group, FarmingActivityId, FarmingActivityName, FarmingActivityRngOffset, value, contribution);

            case ForagingActivityId:
                return new CellCulturalActivity(group, ForagingActivityId, ForagingActivityName, ForagingActivityRngOffset, value, contribution);

            case FishingActivityId:
                return new CellCulturalActivity(group, FishingActivityId, FishingActivityName, FishingActivityRngOffset, value, contribution);
        }

        throw new System.ArgumentException("CellCulturalActivity: Unrecognized activity Id: " + id);
    }

    /// <summary>
    /// Unmerge the activity value from a different culture by a proportion
    /// TODO: Instead of modifying the previous 'new' value, this should use deltas
    /// like prominences do.
    /// </summary>
    /// <param name="activity">the activity from the source culture</param>
    /// <param name="percentage">percentage amount to merge</param>
    public void Unmerge(CulturalActivity activity, float percentage)
    {
        _newValue = MathUtility.UnLerp(_newValue, activity.Value, percentage);
    }

    /// <summary>
    /// Merge the activity value from a different culture by a proportion
    /// TODO: Instead of modifying the previous 'new' value, this should use deltas
    /// like prominences do.
    /// </summary>
    /// <param name="activity">the activity from the source culture</param>
    /// <param name="percentage">percentage amount to merge</param>
    public void Merge(CulturalActivity activity, float percentage)
    {
        _newValue = Mathf.Lerp(_newValue, activity.Value, percentage);
    }

    // This method should be called only once after a Activity is copied from another source group
    public void DecreaseValue(float percentage)
    {
        _newValue = _newValue * percentage;
    }

    public void Update(long timeSpan)
    {
        TerrainCell groupCell = Group.Cell;

        float randomModifier = groupCell.GetNextLocalRandomFloat(RngOffsets.ACTIVITY_UPDATE + RngOffset);
        randomModifier = 1f - (randomModifier * 2f);
        float randomFactor = MaxChangeDelta * randomModifier;

        float maxTargetValue = 1f;
        float minTargetValue = 0f;
        float targetValue = 0;

        if (randomFactor > 0)
        {
            targetValue = Value + (maxTargetValue - Value) * randomFactor;
        }
        else
        {
            targetValue = Value - (minTargetValue - Value) * randomFactor;
        }

        float timeEffect = timeSpan / (timeSpan + TimeEffectConstant);

        _newValue = (Value * (1 - timeEffect)) + (targetValue * timeEffect);
    }

    public void AddPolityProminenceEffect(CulturalActivity polityActivity, PolityProminence polityProminence, long timeSpan)
    {
        float targetValue = polityActivity.Value;
        float prominenceEffect = polityProminence.Value;

        TerrainCell groupCell = Group.Cell;

        int rngOffset = RngOffsets.ACTIVITY_POLITY_PROMINENCE + RngOffset +
            unchecked(polityProminence.Polity.GetHashCode());

        float randomEffect = groupCell.GetNextLocalRandomFloat(rngOffset);

        float timeEffect = timeSpan / (timeSpan + TimeEffectConstant);

        // _newvalue should have been set correctly either by the constructor or by the Update function
        float change = (targetValue - _newValue) * prominenceEffect * timeEffect * randomEffect;

        _newValue = _newValue + change;
    }

    public void PostUpdate()
    {
        Value = Mathf.Clamp01(_newValue);
    }

    public bool CanPerform(CellGroup group)
    {
        if (Id == FishingActivityId)
        {
            return group.Cell.NeighborhoodWaterBiomePresence > 0;
        }

        return true;
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _newValue = Value;
    }
}

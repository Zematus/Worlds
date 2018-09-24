using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellCulturalActivity : CulturalActivity
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

    public const string ForagingActivityId = "ForagingActivity";
    public const string FarmingActivityId = "FarmingActivity";

    public const string ForagingActivityName = "Foraging";
    public const string FarmingActivityName = "Farming";

    public const int ForagingActivityRandomOffset = 0;
    public const int FarmingActivityRandomOffset = 100;

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

    public static CellCulturalActivity CreateForagingActivity(CellGroup group, float value = 0, float contribution = 0)
    {
        return new CellCulturalActivity(group, ForagingActivityId, ForagingActivityName, ForagingActivityRandomOffset, value, contribution);
    }

    public static CellCulturalActivity CreateFarmingActivity(CellGroup group, float value = 0, float contribution = 0)
    {
        return new CellCulturalActivity(group, FarmingActivityId, FarmingActivityName, FarmingActivityRandomOffset, value, contribution);
    }

    public void Merge(CulturalActivity activity, float percentage)
    {
        // _newvalue should have been set correctly either by the constructor or by the Update function
        _newValue = _newValue * (1f - percentage) + activity.Value * percentage;
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

        float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

        _newValue = (Value * (1 - timeEffect)) + (targetValue * timeEffect);
    }

    public void PolityCulturalProminence(CulturalActivity polityActivity, PolityProminence polityProminence, long timeSpan)
    {
        float targetValue = polityActivity.Value;
        float prominenceEffect = polityProminence.Value;

        TerrainCell groupCell = Group.Cell;

        float randomEffect = groupCell.GetNextLocalRandomFloat(RngOffsets.ACTIVITY_POLITY_PROMINENCE + RngOffset + (int)polityProminence.PolityId);

        float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

        // _newvalue should have been set correctly either by the constructor or by the Update function
        float change = (targetValue - _newValue) * prominenceEffect * timeEffect * randomEffect;

        _newValue = _newValue + change;
    }

    public void PostUpdate()
    {
        Value = Mathf.Clamp01(_newValue);
    }
}

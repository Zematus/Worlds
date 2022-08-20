using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CellCulturalPreference : CulturalPreference
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

    [XmlIgnore]
    public CellGroup Group;

    private const float MaxChangeDelta = 0.2f;

    private float _newValue;

#if DEBUG
    // Null ref when loading from file
    // private long _lastUpdateDate = Manager.CurrentWorld.CurrentDate;
#endif

    [XmlIgnore]
    public override float Value
    {
        get => ValueInternal;
        set => _newValue = value;
    }

    public CellCulturalPreference()
    {
    }

    public CellCulturalPreference(CellGroup group, string id, string name, int rngOffset, float value = 0) : base(id, name, rngOffset, value)
    {
        Group = group;
    }

    public static CellCulturalPreference CreateCellInstance(CellGroup group, CulturalPreference basePreference)
    {
        return CreateCellInstance(group, basePreference, basePreference.Value);
    }

    public static CellCulturalPreference CreateCellInstance(CellGroup group, CulturalPreference basePreference, float initialValue)
    {
        return new CellCulturalPreference(group, basePreference.Id, basePreference.Name, basePreference.RngOffset, initialValue);
    }

    /// <summary>
    /// Unmerge the preference value from a different culture by a proportion
    /// TODO: Instead of modifying the previous 'new' value, this should use deltas
    /// like prominences do.
    /// </summary>
    /// <param name="preference">the preference from the source culture</param>
    /// <param name="percentage">percentage amount to merge</param>
    public void Unmerge(CulturalPreference preference, float percentage)
    {
        _newValue = MathUtility.UnLerp(_newValue, preference.Value, percentage);
    }

    /// <summary>
    /// Merge the preference value from a different culture by a proportion
    /// TODO: Instead of modifying the previous 'new' value, this should use deltas
    /// like prominences do.
    /// </summary>
    /// <param name="preference">the preference from the source culture</param>
    /// <param name="percentage">percentage amount to merge</param>
    public void Merge(CulturalPreference preference, float percentage)
    {
        _newValue = Mathf.Lerp(_newValue, preference.Value, percentage);
    }

    // This method should be called only once after a Cultural Value is copied from another source group
    public void DecreaseValue(float percentage)
    {
        _newValue *= percentage;
    }

    public void Update(long timeSpan)
    {
        TerrainCell groupCell = Group.Cell;

        float randomModifier = groupCell.GetNextLocalRandomFloat(RngOffsets.PREFERENCE_UPDATE + RngOffset);
        randomModifier = 1f - (randomModifier * 2f);
        float randomFactor = MaxChangeDelta * randomModifier;

        float maxTargetValue = 1f;
        float minTargetValue = 0f;
        float targetValue;

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

    public void AddPolityProminenceEffect(
        CulturalPreference polityPreference, PolityProminence polityProminence, long timeSpan)
    {
        float targetValue = polityPreference.Value;
        float prominenceEffect = polityProminence.Value;

        TerrainCell groupCell = Group.Cell;

        int rngOffset = RngOffsets.PREFERENCE_POLITY_PROMINENCE + RngOffset +
            unchecked(polityProminence.Polity.GetHashCode());

        float randomEffect = groupCell.GetNextLocalRandomFloat(rngOffset);

        float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

        // _newvalue should have been set correctly either by the constructor or by the Update function
        float change = (targetValue - _newValue) * prominenceEffect * timeEffect * randomEffect;

        _newValue += change;
    }

    public void PostUpdate()
    {
        ValueInternal = Mathf.Clamp01(_newValue);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _newValue = Value;
    }
}

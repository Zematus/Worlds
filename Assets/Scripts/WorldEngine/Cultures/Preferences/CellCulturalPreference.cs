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

    public CellCulturalPreference()
    {
    }

    private CellCulturalPreference(CellGroup group, string id, string name, int rngOffset, float value = 0) : base(id, name, rngOffset, value)
    {
        Group = group;

        _newValue = value;
    }

    public static CellCulturalPreference CreateCellInstance(CellGroup group, CulturalPreference basePreference)
    {
        return CreateCellInstance(group, basePreference, basePreference.Value);
    }

    public static CellCulturalPreference CreateCellInstance(CellGroup group, CulturalPreference basePreference, float initialValue)
    {
        return new CellCulturalPreference(group, basePreference.Id, basePreference.Name, basePreference.RngOffset, initialValue);
    }

    public static CellCulturalPreference CreateAuthorityPreference(CellGroup group, float value = 0)
    {
        return new CellCulturalPreference(group, AuthorityPreferenceId, AuthorityPreferenceName, AuthorityPreferenceRngOffset, value);
    }

    public static CellCulturalPreference CreateCohesionPreference(CellGroup group, float value = 0)
    {
        return new CellCulturalPreference(group, CohesionPreferenceId, CohesionPreferenceName, CohesionPreferenceRngOffset, value);
    }

    public static CellCulturalPreference CreateIsolationPreference(CellGroup group, float value = 0)
    {
        return new CellCulturalPreference(group, IsolationPreferenceId, IsolationPreferenceName, IsolationPreferenceRngOffset, value);
    }

    public void Merge(CulturalPreference preference, float percentage)
    {
        // _newvalue should have been set correctly either by the constructor or by the Update function
        _newValue = _newValue * (1f - percentage) + preference.Value * percentage;
    }

    // This method should be called only once after a Cultural Value is copied from another source group
    public void DecreaseValue(float percentage)
    {
        _newValue = _newValue * percentage;
    }

    public void Update(long timeSpan)
    {
        TerrainCell groupCell = Group.Cell;

        float randomModifier = groupCell.GetNextLocalRandomFloat(RngOffsets.PREFERENCE_UPDATE + RngOffset);
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

        _newValue = _newValue + change;
    }

    public void PostUpdate()
    {
        Value = Mathf.Clamp01(_newValue);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _newValue = Value;
    }
}

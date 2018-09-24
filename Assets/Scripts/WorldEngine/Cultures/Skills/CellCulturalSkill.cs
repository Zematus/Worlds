using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class CellCulturalSkill : CulturalSkill, ISynchronizable
{
    [XmlAttribute]
    public float AdaptationLevel;

    [XmlIgnore]
    public CellGroup Group;

    public float _newValue;

    public CellCulturalSkill()
    {
    }

    protected CellCulturalSkill(CellGroup group, string id, string name, int rngOffset, float value = 0) : base(id, name, rngOffset, value)
    {
        Group = group;

        _newValue = value;
    }

    public static CellCulturalSkill CreateCellInstance(CellGroup group, CulturalSkill baseSkill)
    {
        return CreateCellInstance(group, baseSkill, baseSkill.Value);
    }

    public static CellCulturalSkill CreateCellInstance(CellGroup group, CulturalSkill baseSkill, float initialValue)
    {
        if (BiomeSurvivalSkill.IsBiomeSurvivalSkill(baseSkill))
        {
            return new BiomeSurvivalSkill(group, baseSkill, initialValue);
        }

        if (SeafaringSkill.IsSeafaringSkill(baseSkill))
        {
            return new SeafaringSkill(group, baseSkill, initialValue);
        }

        throw new System.Exception("Unhandled CulturalSkill type: " + baseSkill.Id);
    }

    public void Merge(CulturalSkill skill, float percentage)
    {
        // _newvalue should have been set correctly either by the constructor or by the Update function
        float value = _newValue * (1f - percentage) + skill.Value * percentage;

        //		#if DEBUG
        //		if (Manager.RegisterDebugEvent != null) {
        //			if (Group.Id == Manager.TracingData.GroupId) {
        //
        //				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"Merge - Group:" + groupId,
        //					"CurrentDate: " + Group.World.CurrentDate + 
        //					", Name: " + Name + 
        //					", Value: " + Value + 
        //					", source Value: " + skill.Value + 
        //					", percentage: " + percentage + 
        //					", new value: " + value + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

        _newValue = value;
    }

    // This method should be called only once after a Skill is copied from another source group
    public void DecreaseValue(float percentage)
    {
        float value = _newValue * percentage;

        //		#if DEBUG
        //		if (Manager.RegisterDebugEvent != null) {
        //			if (Group.Id == Manager.TracingData.GroupId) {
        //
        //				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"ModifyValue - Group:" + groupId,
        //					"CurrentDate: " + Group.World.CurrentDate + 
        //					", Name: " + Name + 
        //					", Value: " + Value + 
        //					", percentage: " + percentage + 
        //					", new value: " + value + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

        _newValue = value;
    }

    public virtual void Synchronize()
    {

    }

    public virtual void FinalizeLoad()
    {

    }

    public abstract void Update(long timeSpan);

    protected void UpdateInternal(long timeSpan, float timeEffectFactor, float specificModifier)
    {
        TerrainCell groupCell = Group.Cell;

        float randomModifier = groupCell.GetNextLocalRandomFloat(RngOffsets.SKILL_UPDATE + RngOffset);
        randomModifier *= randomModifier;
        float randomFactor = specificModifier - randomModifier;

        float maxTargetValue = 1.0f;
        float minTargetValue = -0.2f;
        float targetValue = 0;

        if (randomFactor > 0)
        {
            targetValue = Value + (maxTargetValue - Value) * randomFactor;
        }
        else
        {
            targetValue = Value - (minTargetValue - Value) * randomFactor;
        }

        float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

        float newValue = (Value * (1 - timeEffect)) + (targetValue * timeEffect);

        //		#if DEBUG
        //		if (Manager.RegisterDebugEvent != null) {
        //			if (Group.Id == Manager.TracingData.GroupId) {
        //
        //				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"UpdateInternal - Group:" + groupId,
        //					"CurrentDate: " + Group.World.CurrentDate + 
        //					", Name: " + Name + 
        //					", timeSpan: " + timeSpan + 
        //					", timeEffectFactor: " + timeEffectFactor + 
        //					", specificModifier: " + specificModifier + 
        //					", randomModifier: " + randomModifier + 
        //					", targetValue: " + targetValue + 
        //					", Value: " + Value + 
        //					", newValue: " + newValue + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

        _newValue = newValue;
    }

    public abstract void PolityCulturalProminence(CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan);

    protected void PolityCulturalProminenceInternal(CulturalSkill politySkill, PolityProminence polityProminence, long timeSpan, float timeEffectFactor)
    {
        float targetValue = politySkill.Value;
        float prominenceEffect = polityProminence.Value;

        TerrainCell groupCell = Group.Cell;

        float randomEffect = groupCell.GetNextLocalRandomFloat(RngOffsets.SKILL_POLITY_PROMINENCE + RngOffset + (int)polityProminence.PolityId);

        float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

        // _newvalue should have been set correctly either by the constructor or by the Update function
        float change = (targetValue - _newValue) * prominenceEffect * timeEffect * randomEffect;

        //		#if DEBUG
        //		if (Manager.RegisterDebugEvent != null) {
        //			if (Group.Id == Manager.TracingData.GroupId) {
        //
        //				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"PolityCulturalProminenceInternal - Group:" + groupId,
        //					"CurrentDate: " + Group.World.CurrentDate + 
        //					", Name: " + Name + 
        //					", timeSpan: " + timeSpan + 
        //					", timeEffectFactor: " + timeEffectFactor + 
        //					", randomEffect: " + randomEffect + 
        //					", polity Id: " + polityProminence.PolityId + 
        //					", polityProminence.Value: " + prominenceEffect + 
        //					", politySkill.Value: " + targetValue + 
        //					", Value: " + Value + 
        //					", change: " + change + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

        _newValue = _newValue + change;
    }

    protected void RecalculateAdaptation(float targetValue)
    {
        AdaptationLevel = MathUtility.RoundToSixDecimals(1 - Mathf.Abs(Value - targetValue));
    }

    public void PostUpdate()
    {
        Value = MathUtility.RoundToSixDecimals(Mathf.Clamp01(_newValue));

        PostUpdateInternal();
    }

    protected abstract void PostUpdateInternal();
}

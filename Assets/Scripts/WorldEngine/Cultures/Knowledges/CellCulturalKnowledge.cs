using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public abstract class CellCulturalKnowledge : CulturalKnowledge
{
//#if DEBUG
//    [XmlIgnore]
//    public long AcquisitionDate = -1; // This property is used for debugging purposes
//#endif

    public const float MinProgressLevel = 0.001f;

    [XmlAttribute("RO")]
    public int InstanceRngOffset;

    [XmlAttribute("L")]
    public int Limit = -1;

    [XmlIgnore]
    public CellGroup Group;

    protected int _newValue;

    private Knowledge _referenceKnowledge; // TODO: remove when 'Knowledge' replaces 'CellCulturalKnowledge' (requires Knowledge modding)

    public CellCulturalKnowledge()
    {

    }

    public CellCulturalKnowledge(
        CellGroup group,
        string id,
        string name,
        int typeRngOffset,
        int value,
        int limit) :
        base(id, name, value)
    {
        Group = group;
        InstanceRngOffset = typeRngOffset;

        _newValue = value;

        _referenceKnowledge = Knowledge.GetKnowledge(id);

//#if DEBUG
//        AcquisitionDate = group.World.CurrentDate;

//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (Group.Id == Manager.TracingData.GroupId)
//            {
//                string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "CellCulturalKnowledge.CellCulturalKnowledge - Group:" + groupId,
//                    "CurrentDate: " + Group.World.CurrentDate +
//                    ", Id: " + Id +
//                    ", IsPresent: " + IsPresent +
//                    ", Value: " + Value +
//                    ", _newValue: " + _newValue +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        SetLimit(limit);
    }

    public void SetLevelLimit(int levelLimit)
    {
        if (levelLimit > Limit)
        {
            SetLimit(levelLimit);
        }
    }

    public void SetLimit(int limit)
    {
        if (!limit.IsInsideRange(MinLimitValue, MaxLimitValue))
        {
            Debug.LogWarning("CulturalKnowledge: Limit can't be set below " + ScaledMinLimitValue + " or above " + ScaledMaxLimitValue + ", id: " + Id + ", limit: " + (limit * MathUtility.IntToFloatScalingFactor));

            limit = Mathf.Clamp(limit, MinLimitValue, MaxLimitValue);
        }

        Limit = limit;

        UpdateProgressLevel();

        SetHighestLimit(limit);
    }

    public float ScaledLimit
    {
        get { return Limit * MathUtility.IntToFloatScalingFactor; }
    }

    public void UpdateProgressLevel()
    {
        ProgressLevel = 0;

        if (Limit > 0)
            ProgressLevel = MathUtility.RoundToSixDecimals(Mathf.Clamp01(Value / (float)Limit));

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Group.Id == Manager.TracingData.GroupId)
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "CellCulturalKnowledge.UpdateProgressLevel - Knowledge.Id:" + Id + ", Group.Id:" + Group.Id,
        //                    "CurrentDate: " + Group.World.CurrentDate +
        //                    ", ProgressLevel: " + ProgressLevel +
        //                    ", Value: " + Value +
        //                    ", Limit: " + Limit +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif
    }

    public void ModifyLevelLimit(int levelLimitDelta)
    {
        if (Limit == -1)
        {
            throw new System.Exception("CellCulturalKnowledge - ModifyLevelLimit: Limit is unset");
        }

        SetLimit(Limit + levelLimitDelta);
    }

    public static CellCulturalKnowledge CreateCellInstance(string id, CellGroup group, int initialValue, int initialLimit = -1)
    {
        switch (id)
        {
            case ShipbuildingKnowledge.KnowledgeId:

                if (initialLimit == -1)
                    initialLimit = ShipbuildingKnowledge.BaseLimit;

                return new ShipbuildingKnowledge(group, initialValue, initialLimit);

            case AgricultureKnowledge.KnowledgeId:

                if (initialLimit == -1)
                    initialLimit = AgricultureKnowledge.BaseLimit;

                return new AgricultureKnowledge(group, initialValue, initialLimit);

            case SocialOrganizationKnowledge.KnowledgeId:

                if (initialLimit == -1)
                    initialLimit = SocialOrganizationKnowledge.BaseLimit;

                return new SocialOrganizationKnowledge(group, initialValue, initialLimit);
        }

        throw new System.Exception("Unexpected CulturalKnowledge type: " + id);
    }

    public void Merge(int value, float percentage)
    {
        // _newvalue should have been set correctly either by the constructor or by the Update function
        int mergedValue = MathUtility.LerpToIntAndGetDecimals(_newValue, value, percentage, out float d);

        if (d > Group.GetNextLocalRandomFloat(RngOffsets.KNOWLEDGE_MERGE + InstanceRngOffset))
            mergedValue++;

#if DEBUG
        if ((Id == SocialOrganizationKnowledge.KnowledgeId) && (mergedValue < SocialOrganizationKnowledge.MinValueForTribeFormation))
        {
            if (Group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning("group with low social organization has faction cores - Id: " + Group);
            }
        }
#endif

        _newValue = mergedValue;
    }

    public void Update(long timeSpan)
    {
        UpdateInternal(timeSpan);

        foreach (ICellGroupEventGenerator generator in _referenceKnowledge.OnUpdateEventGenerators)
        {
            generator.TryGenerateEventAndAssign(Group);
        }
    }

    protected void UpdateValueInternal(long timeSpan, float timeEffectFactor, float specificModifier)
    {
        TerrainCell groupCell = Group.Cell;

        int rngOffset = RngOffsets.KNOWLEDGE_UPDATE_VALUE_INTERNAL + InstanceRngOffset;

        float randomModifier = groupCell.GetNextLocalRandomFloat(rngOffset++);
        randomModifier *= randomModifier;
        float randomFactor = specificModifier - randomModifier;
        randomFactor = Mathf.Clamp(randomFactor, -1, 1);

        int maxTargetValue = Limit;
        int minTargetValue = 0;
        int targetValue = 0;

        if (randomFactor > 0)
        {
            targetValue = Value + (int)((maxTargetValue - Value) * randomFactor);
        }
        else
        {
            targetValue = Value - (int)((minTargetValue - Value) * randomFactor);
        }

        float timeEffect = timeSpan / (timeSpan + timeEffectFactor);

        float d;
        int newValue = MathUtility.LerpToIntAndGetDecimals(Value, targetValue, timeEffect, out d);

        if (d > Group.GetNextLocalRandomFloat(rngOffset++))
            newValue++;

#if DEBUG
        if ((Limit > 1) && (newValue > Limit) && (newValue > Value))
        {
            throw new System.Exception("UpdateValueInternal: new value " + newValue + " above Level Limit " + Limit);
        }

        if (newValue > 1000000)
        {
            throw new System.Exception("UpdateValueInternal: new value " + newValue + " above 1000000");
        }

        if ((Id == SocialOrganizationKnowledge.KnowledgeId) && (newValue < SocialOrganizationKnowledge.MinValueForTribeFormation))
        {
            if (Group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning("Group with low social organization has faction cores - Id: " + Group + ", newValue:" + newValue);
            }

            if (Group.WillBecomeCoreOfFaction != null)
            {
                Debug.LogWarning("Group with low social organization will become a faction core - Id: " + Group + ", newValue:" + newValue);
            }
        }
#endif

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Group.Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "CellCulturalKnowledge.UpdateValueInternal - Group:" + groupId,
        //                    "CurrentDate: " + Group.World.CurrentDate +
        //                    ", Id: " + Id +
        //                    ", IsPresent: " + IsPresent +
        //                    ", Value: " + Value +
        //                    ", _newValue: " + _newValue +
        //                    ", newValue: " + newValue +
        //                    ", targetValue: " + targetValue +
        //                    ", Limit: " + Limit +
        //                    ", randomModifier: " + randomModifier +
        //                    ", specificModifier: " + specificModifier +
        //                    ", InstanceRngOffset: " + InstanceRngOffset +
        //                    ", timeEffect: " + timeEffect +
        //                    //", AcquisitionDate: " + AcquisitionDate +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        _newValue = newValue;
    }

    public abstract void AddPolityProminenceEffect(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan);

    protected void AddPolityProminenceEffectInternal(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan, float timeEffectFactor)
    {
        int rngOffset = RngOffsets.KNOWLEDGE_POLITY_PROMINENCE + InstanceRngOffset +
            unchecked(polityProminence.Polity.GetHashCode());

        int targetValue = polityKnowledge.Value;
        float prominenceEffect = polityProminence.Value;

        TerrainCell groupCell = Group.Cell;

        float randomEffect = groupCell.GetNextLocalRandomFloat(rngOffset++);

        float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

        int valueDelta = targetValue - _newValue;

        float d;
        // _newvalue should have been set correctly either by the constructor or by the Update function
        int valueChange = (int)MathUtility.MultiplyAndGetDecimals(valueDelta, prominenceEffect * timeEffect * randomEffect, out d);

        if (d > Group.GetNextLocalRandomFloat(rngOffset++))
            valueChange++;

        //#if DEBUG
        //        if (Manager.RegisterDebugEvent != null)
        //        {
        //            if (Manager.TracingData.Priority <= 0)
        //            {
        //                if (Group.Id == Manager.TracingData.GroupId)
        //                {
        //                    string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

        //                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                        "CellCulturalKnowledge.PolityCulturalProminenceInternal - Group:" + groupId,
        //                        "CurrentDate: " + Group.World.CurrentDate +
        //                        ", Name: " + Name +
        //                        ", timeSpan: " + timeSpan +
        //                        ", timeEffectFactor: " + timeEffectFactor +
        //                        ", randomEffect: " + randomEffect +
        //                        ", Group.PolityProminences.Count: " + Group.PolityProminences.Count +
        //                        ", polity Id: " + polityProminence.PolityId +
        //                        ", polityProminence.Value: " + prominenceEffect +
        //                        ", politySkill.Value: " + targetValue +
        //                        ", Value: " + Value +
        //                        ", _newValue: " + _newValue +
        //                        ", valueChange: " + valueChange +
        //                        "", Group.World.CurrentDate);

        //                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //                }
        //            }
        //            else if (Manager.TracingData.Priority <= 1)
        //            {
        //                string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "CellCulturalKnowledge.PolityCulturalProminenceInternal - Group:" + groupId,
        //                    "CurrentDate: " + Group.World.CurrentDate +
        //                    "", Group.World.CurrentDate);

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        _newValue = _newValue + valueChange;
    }

    public void PostUpdate()
    {
        Value = _newValue;

        UpdateProgressLevel();
    }

    public abstract float CalculateExpectedProgressLevel();
    public abstract float CalculateTransferFactor();

    protected abstract void UpdateInternal(long timeSpan);

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _referenceKnowledge = Knowledge.GetKnowledge(Id);

        _newValue = Value;
    }
}

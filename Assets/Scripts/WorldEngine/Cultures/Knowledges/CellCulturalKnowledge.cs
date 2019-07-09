using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public abstract class CellCulturalKnowledge : CulturalKnowledge
{
#if DEBUG
    [XmlIgnore]
    public long AcquisitionDate = -1; // This property is used for debugging purposes
#endif

    public const float MinProgressLevel = 0.001f;

    [XmlAttribute("PL")]
    public float ProgressLevel;

    [XmlAttribute("A")]
    public int Limit;

    [XmlAttribute("RO")]
    public int InstanceRngOffset;

    [XmlIgnore]
    public CellGroup Group;

    protected int _newValue;

    private Knowledge _referenceKnowledge; // TODO: remove when 'Knowledge' replaces 'CellCulturalKnowledge'

    public float ScaledLimit
    {
        get { return Limit * ValueScaleFactor; }
    }

    public CellCulturalKnowledge()
    {

    }

    public CellCulturalKnowledge(
        CellGroup group, 
        string id, 
        string name, 
        int typeRngOffset, 
        int value, 
        List<string> limitLevelIds) : 
        base(id, name, value, limitLevelIds)
    {
        Group = group;
        InstanceRngOffset = typeRngOffset;

        _newValue = value;

        LoadLevelLimits();

        _referenceKnowledge = Knowledge.GetKnowledge(id);

#if DEBUG
        AcquisitionDate = group.World.CurrentDate;
        
        //if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //{
        //    if (Group.Id == Manager.TracingData.GroupId)
        //    {
        //        string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

        //        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //            "CellCulturalKnowledge.CellCulturalKnowledge - Group:" + groupId,
        //            "CurrentDate: " + Group.World.CurrentDate +
        //            ", Id: " + Id +
        //            ", IsPresent: " + IsPresent +
        //            ", Value: " + Value +
        //            ", _newValue: " + _newValue +
        //            "");

        //        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //    }
        //}
#endif
    }

    public void Initialize(int value, List<string> levelLimitIds)
    {
        Value = value;
        _newValue = value;

        if (levelLimitIds != null)
        {
            foreach (string levelId in levelLimitIds)
            {
                AddLevelLimitId(levelId);
            }

            LoadLevelLimits();
        }

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Group.Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "CellCulturalKnowledge.SetInitialValue - Group:" + groupId,
        //                    "CurrentDate: " + Group.World.CurrentDate +
        //                    ", Id: " + Id +
        //                    ", IsPresent: " + IsPresent +
        //                    ", Value: " + Value +
        //                    ", _newValue: " + _newValue +
        //                    ", AcquisitionDate: " + AcquisitionDate +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif
    }

    public void AddLevelLimit(string id, int levelLimit)
    {
        if (!AddLevelLimitId(id))
            return;

        if (levelLimit > Limit)
        {
            Limit = levelLimit;

            UpdateProgressLevel();

            SetHighestLimit(Limit);
        }
    }

    public void LoadLevelLimits()
    {
        Limit = GetBaseLimit();

        foreach (string id in LevelLimitIds)
        {
            Limit = Mathf.Max(Limit, World.GetKnowledgeLevelLimit(id));
        }
    }

    public void ModifyLevelLimit(int LevelLimitIncrease)
    {
        Limit += LevelLimitIncrease;

        UpdateProgressLevel();

        SetHighestLimit(Limit);
    }

    public static CellCulturalKnowledge CreateCellInstance(string id, CellGroup group, int initialValue = 0, List<string> levelLimitIds = null)
    {
        switch (id)
        {
            case ShipbuildingKnowledge.KnowledgeId:
                return new ShipbuildingKnowledge(group, initialValue, levelLimitIds);

            case AgricultureKnowledge.KnowledgeId:
                return new AgricultureKnowledge(group, initialValue, levelLimitIds);

            case SocialOrganizationKnowledge.KnowledgeId:
                return new SocialOrganizationKnowledge(group, initialValue, levelLimitIds);
        }

        throw new System.Exception("Unexpected CulturalKnowledge type: " + id);
    }

    public int GetHighestLimit()
    {
        System.Type knowledgeType = this.GetType();

        System.Reflection.FieldInfo fInfo = knowledgeType.GetField("HighestLimit"); // TODO: avoid using reflection

        return (int)fInfo.GetValue(this);
    }

    public void SetHighestLimit(int value)
    {
        System.Type knowledgeType = this.GetType();

        System.Reflection.FieldInfo fInfo = knowledgeType.GetField("HighestLimit"); // TODO: avoid using reflection

        int currentValue = (int)fInfo.GetValue(this);

        if (value > currentValue)
        {
            fInfo.SetValue(this, value);
        }
    }

    public void Merge(int value, float percentage)
    {
        float d;
        // _newvalue should have been set correctly either by the constructor or by the Update function
        int mergedValue = MathUtility.LerpToIntAndGetDecimals(_newValue, value, percentage, out d);

        if (d > Group.GetNextLocalRandomFloat(RngOffsets.KNOWLEDGE_MERGE + InstanceRngOffset))
            mergedValue++;

#if DEBUG
        if ((Id == SocialOrganizationKnowledge.KnowledgeId) && (mergedValue < SocialOrganizationKnowledge.MinValueForHoldingTribalism))
        {
            if (Group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning("group with low social organization has faction cores - Id: " + Group.Id);
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
//                    "CellCulturalKnowledge.Merge - Group:" + groupId,
//                    "CurrentDate: " + Group.World.CurrentDate +
//                    ", Id: " + Id +
//                    ", IsPresent: " + IsPresent +
//                    ", Value: " + Value +
//                    ", _newValue: " + _newValue +
//                    ", mergedValue: " + mergedValue +
//                    ", value (param): " + value +
//                    ", percentage: " + percentage +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        _newValue = mergedValue;
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

    public void CalculateLimit()
    {
        LoadLevelLimits();

        UpdateProgressLevel();

        SetHighestLimit(Limit);
    }

    public void RecalculateLimit()
    {
        LoadLevelLimits();

        UpdateProgressLevel();

        SetHighestLimit(Limit);
    }

    public void RecalculateLimit_Old() // Replace with Non-"Old" and remove CalculateLimitInternal
    {
        LoadLevelLimits();

        foreach (CulturalDiscovery d in Group.Culture.Discoveries.Values)
        {
            Limit = Mathf.Max(CalculateLimitInternal(d), Limit);
        }

        UpdateProgressLevel();

        SetHighestLimit(Limit);
    }

    public void CalculateLimit(CulturalDiscovery discovery)
    {
        int newLimit = CalculateLimitInternal(discovery);

        if (newLimit > Limit)
        {
            Limit = newLimit;

            UpdateProgressLevel();

            SetHighestLimit(Limit);
        }
    }

    public void Update(long timeSpan)
    {
        UpdateInternal(timeSpan);

        foreach (ICellGroupEventGenerator generator in _referenceKnowledge.OnUpdateEventGenerators)
        {
            if (generator.CanAssignEventTypeToGroup(Group))
            {
                generator.GenerateAndAssignEvent(Group);
            }
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
            throw new System.Exception("UpdateValueInternal: new value " + newValue + " above 1000000000");
        }
#endif

#if DEBUG
        if ((Id == SocialOrganizationKnowledge.KnowledgeId) && (newValue < SocialOrganizationKnowledge.MinValueForHoldingTribalism))
        {
            if (Group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning("Group with low social organization has faction cores - Id: " + Group.Id + ", newValue:" + newValue);
            }

            if (Group.WillBecomeFactionCore)
            {
                Debug.LogWarning("Group with low social organization will become a faction core - Id: " + Group.Id + ", newValue:" + newValue);
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

    public abstract void PolityCulturalProminence(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan);

    protected void PolityCulturalProminenceInternal(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan, float timeEffectFactor)
    {
        int rngOffset = RngOffsets.KNOWLEDGE_POLITY_PROMINENCE + InstanceRngOffset + unchecked((int)polityProminence.PolityId);

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

    public abstract bool WillBeLost();

    public virtual void LossConsequences()
    {

    }

    protected abstract void UpdateInternal(long timeSpan);
    protected abstract int CalculateLimitInternal(CulturalDiscovery discovery); // TODO: Get rid of this method
    protected abstract int GetBaseLimit(); // TODO: Get rid of this method

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _newValue = Value;
    }
}

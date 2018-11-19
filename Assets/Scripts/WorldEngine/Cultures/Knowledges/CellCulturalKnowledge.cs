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
    public int Asymptote;

    [XmlAttribute("RO")]
    public int InstanceRngOffset;

    [XmlIgnore]
    public CellGroup Group;

    protected int _newValue;

    public float ScaledAsymptote
    {
        get { return Asymptote * ValueScaleFactor; }
    }

    public CellCulturalKnowledge()
    {

    }

    public CellCulturalKnowledge(CellGroup group, string id, string name, int typeRngOffset, int value) : base(id, name, value)
    {
        Group = group;
        //InstanceRngOffset = unchecked((int)group.GenerateUniqueIdentifier(group.World.CurrentDate, 100L, typeRngOffset));
        InstanceRngOffset = typeRngOffset;

        _newValue = value;

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

    public CellCulturalKnowledge(CellGroup group, string id, string name, int typeRngOffset, int value, int asymptote) : base(id, name, value)
    {
        Group = group;
        //InstanceRngOffset = unchecked((int)group.GenerateUniqueIdentifier(group.World.CurrentDate, 100L, typeRngOffset));
        InstanceRngOffset = typeRngOffset;
        Asymptote = asymptote;

        _newValue = value;

#if DEBUG
        AcquisitionDate = group.World.CurrentDate;

        //if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //{
        //    if (Group.Id == Manager.TracingData.GroupId)
        //    {
        //        string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

        //        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //            "CellCulturalKnowledge.CellCulturalKnowledge (with asymptote) - Group:" + groupId,
        //            "CurrentDate: " + Group.World.CurrentDate +
        //            ", Id: " + Id +
        //            ", IsPresent: " + IsPresent +
        //            ", Value: " + Value +
        //            ", _newValue: " + _newValue +
        //            ", Asymptote: " + Asymptote +
        //            "");

        //        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //    }
        //}
#endif
    }

    public void SetInitialValue(int value)
    {
        Value = value;
        _newValue = value;

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

    public static CellCulturalKnowledge CreateCellInstance(string id, CellGroup group, int initialValue = 0)
    {
        switch (id)
        {
            case ShipbuildingKnowledge.KnowledgeId:
                return new ShipbuildingKnowledge(group, initialValue);

            case AgricultureKnowledge.KnowledgeId:
                return new AgricultureKnowledge(group, initialValue);

            case SocialOrganizationKnowledge.KnowledgeId:
                return new SocialOrganizationKnowledge(group, initialValue);
        }

        throw new System.Exception("Unexpected CulturalKnowledge type: " + id);
    }

    public int GetHighestAsymptote()
    {
        System.Type knowledgeType = this.GetType();

        System.Reflection.FieldInfo fInfo = knowledgeType.GetField("HighestAsymptote");

        return (int)fInfo.GetValue(this);
    }

    public void SetHighestAsymptote(int value)
    {
        System.Type knowledgeType = this.GetType();

        System.Reflection.FieldInfo fInfo = knowledgeType.GetField("HighestAsymptote");

        int currentValue = (int)fInfo.GetValue(this);
        fInfo.SetValue(this, Mathf.Max(value, currentValue));
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

        if (Asymptote > 0)
            ProgressLevel = MathUtility.RoundToSixDecimals(Mathf.Clamp01(Value / (float)Asymptote));

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
//                    ", Asymptote: " + Asymptote +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif
    }

    public void CalculateAsymptote()
    {
        Asymptote = GetBaseAsymptote();

        UpdateProgressLevel();

        SetHighestAsymptote(Asymptote);
    }

    public void RecalculateAsymptote()
    {
        Asymptote = GetBaseAsymptote();

        foreach (CulturalDiscovery d in Group.Culture.Discoveries.Values)
        {
            if (d.IsPresent)
            {
                Asymptote = Mathf.Max(CalculateAsymptoteInternal(d), Asymptote);
            }
        }

        UpdateProgressLevel();

        SetHighestAsymptote(Asymptote);
    }

    public void CalculateAsymptote(CulturalDiscovery discovery)
    {
        int newAsymptote = CalculateAsymptoteInternal(discovery);

        if (newAsymptote > Asymptote)
        {
            Asymptote = newAsymptote;

            UpdateProgressLevel();

            SetHighestAsymptote(Asymptote);
        }
    }

    public void Update(long timeSpan)
    {
        UpdateInternal(timeSpan);
    }

    protected void UpdateValueInternal(long timeSpan, float timeEffectFactor, float specificModifier)
    {
        TerrainCell groupCell = Group.Cell;

        int rngOffset = RngOffsets.KNOWLEDGE_UPDATE_VALUE_INTERNAL + InstanceRngOffset;

        float randomModifier = groupCell.GetNextLocalRandomFloat(rngOffset++);
        randomModifier *= randomModifier;
        float randomFactor = specificModifier - randomModifier;
        randomFactor = Mathf.Clamp(randomFactor, -1, 1);

        int maxTargetValue = Asymptote;
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
        if ((Asymptote > 1) && (newValue > Asymptote) && (newValue > Value))
        {
            throw new System.Exception("UpdateValueInternal: new value " + newValue + " above Asymptote " + Asymptote);
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
//                    ", Asymptote: " + Asymptote +
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

#if DEBUG
        if (Manager.RegisterDebugEvent != null)
        {
            if (Manager.TracingData.Priority <= 0)
            {
                if (Group.Id == Manager.TracingData.GroupId)
                {
                    string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                        "CellCulturalKnowledge.PolityCulturalProminenceInternal - Group:" + groupId,
                        "CurrentDate: " + Group.World.CurrentDate +
                        ", Name: " + Name +
                        ", timeSpan: " + timeSpan +
                        ", timeEffectFactor: " + timeEffectFactor +
                        ", randomEffect: " + randomEffect +
                        ", Group.PolityProminences.Count: " + Group.PolityProminences.Count +
                        ", polity Id: " + polityProminence.PolityId +
                        ", polityProminence.Value: " + prominenceEffect +
                        ", politySkill.Value: " + targetValue +
                        ", Value: " + Value +
                        ", _newValue: " + _newValue +
                        ", valueChange: " + valueChange +
                        "", Group.World.CurrentDate);

                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                }
            }
            else if (Manager.TracingData.Priority <= 1)
            {
                string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "CellCulturalKnowledge.PolityCulturalProminenceInternal - Group:" + groupId,
                    "CurrentDate: " + Group.World.CurrentDate +
                    "", Group.World.CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

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
    public abstract void LossConsequences();

    protected abstract void UpdateInternal(long timeSpan);
    protected abstract int CalculateAsymptoteInternal(CulturalDiscovery discovery);
    protected abstract int GetBaseAsymptote();

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _newValue = Value;
    }
}

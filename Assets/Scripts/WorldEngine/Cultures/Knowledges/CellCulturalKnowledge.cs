using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public abstract class CellCulturalKnowledge : CulturalKnowledge
{
//#if DEBUG
//    [XmlIgnore]
//    public long AcquisitionDate = -1; // This property is used for debugging purposes
//#endif

    public const float MinProgressLevel = 0.001f;

    [XmlAttribute("RO")]
    public int InstanceRngOffset;

    [XmlIgnore]
    public CellGroup Group;

    [XmlIgnore]
    public float ProgressLevel => (Limit.Value > 0) ? Mathf.Clamp01(Value / Limit.Value) : 0;

    protected float _newValue;

    private Knowledge _referenceKnowledge; // TODO: remove when 'Knowledge' replaces 'CellCulturalKnowledge' (requires Knowledge modding)

    [XmlIgnore]
    public KnowledgeLimit Limit;

    public CellCulturalKnowledge()
    {

    }

    public CellCulturalKnowledge(
        CellGroup group,
        string id,
        string name,
        int typeRngOffset,
        float value,
        KnowledgeLimit limit) :
        base(id, name, value)
    {
        Group = group;
        InstanceRngOffset = typeRngOffset;

        _newValue = value;
        Limit = limit;

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
    }

    public static CellCulturalKnowledge CreateCellInstance(string id, CellGroup group, float initialValue, KnowledgeLimit limit, float initialLimit = -1)
    {
        limit.SetValue(GetInitialLimit(id, initialLimit));

        switch (id)
        {
            case ShipbuildingKnowledge.KnowledgeId:
                return new ShipbuildingKnowledge(group, initialValue, limit);

            case AgricultureKnowledge.KnowledgeId:
                return new AgricultureKnowledge(group, initialValue, limit);

            case SocialOrganizationKnowledge.KnowledgeId:
                return new SocialOrganizationKnowledge(group, initialValue, limit);
        }

        throw new System.Exception("Unexpected CulturalKnowledge type: " + id);
    }

    public static float GetInitialLimit(string id, float initialLimit = -1)
    {
        switch (id)
        {
            case ShipbuildingKnowledge.KnowledgeId:
                if (initialLimit == -1)
                    initialLimit = ShipbuildingKnowledge.BaseLimit;
                break;

            case AgricultureKnowledge.KnowledgeId:
                if (initialLimit == -1)
                    initialLimit = AgricultureKnowledge.BaseLimit;
                break;

            case SocialOrganizationKnowledge.KnowledgeId:
                if (initialLimit == -1)
                    initialLimit = SocialOrganizationKnowledge.BaseLimit;
                break;

            default:
                throw new System.Exception("Unexpected CulturalKnowledge type: " + id);
        }

        return initialLimit;
    }

    public void Merge(float value, float percentage)
    {
        // _newvalue should have been set correctly either by the constructor or by the Update function
        float mergedValue = Mathf.Lerp(_newValue, value, percentage);

#if DEBUG
        if ((Id == SocialOrganizationKnowledge.KnowledgeId) && (mergedValue < TribeFormationEvent.MinSocialOrganizationKnowledgeValue))
        {
            if (Group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning($"group with low social organization has faction cores - Id: {Group}");
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

        float maxTargetValue = Limit.Value;
        float minTargetValue = 0;
        float targetValue = 0;

        if (randomFactor > 0)
        {
            targetValue = Value + (int)((maxTargetValue - Value) * randomFactor);
        }
        else
        {
            targetValue = Value - (int)((minTargetValue - Value) * randomFactor);
        }

        float timeEffect = timeSpan / (timeSpan + timeEffectFactor);

        float newValue = Mathf.Lerp(Value, targetValue, timeEffect);

#if DEBUG
        if ((Limit.Value > 1) && (newValue > Limit.Value) && (newValue > Value))
        {
            throw new System.Exception($"UpdateValueInternal: new value {newValue} above level limit {Limit.Value}");
        }

        if (newValue > KnowledgeLimit.MaxLimitValue)
        {
            throw new System.Exception($"UpdateValueInternal: new value {newValue} above {KnowledgeLimit.MaxLimitValue}");
        }

        if ((Id == SocialOrganizationKnowledge.KnowledgeId) && (newValue < TribeFormationEvent.MinSocialOrganizationKnowledgeValue))
        {
            if (Group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning($"Group with low social organization has faction cores - Id: {Group}, newValue:{newValue}");
            }

            if (Group.WillBecomeCoreOfFaction != null)
            {
                Debug.LogWarning($"Group with low social organization will become a faction core - Id: {Group}, newValue:{newValue}");
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

        float targetValue = polityKnowledge.Value;
        float prominenceEffect = polityProminence.Value;

        TerrainCell groupCell = Group.Cell;

        float randomEffect = groupCell.GetNextLocalRandomFloat(rngOffset++);

        float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

        float valueDelta = targetValue - _newValue;

        // _newvalue should have been set correctly either by the constructor or by the Update function
        float valueChange = valueDelta * prominenceEffect * timeEffect * randomEffect;

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

        _newValue += valueChange;
    }

    public void PostUpdate()
    {
        if (Value != _newValue)
        {
            Value = _newValue;

            Group.GenerateKnowledgeLevelFallsBelowEvents(Id, Value);
            Group.GenerateKnowledgeLevelRaisesAboveEvents(Id, Value);
        }
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

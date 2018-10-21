using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public abstract class CellCulturalKnowledge : CulturalKnowledge, ISynchronizable
{
    public const float MinProgressLevel = 0.001f;

    [XmlAttribute("PL")]
    public float ProgressLevel;

    [XmlAttribute("A")]
    public int Asymptote;

    [XmlAttribute("RO")]
    public int RngOffset;

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
        RngOffset = unchecked((int)group.GenerateUniqueIdentifier(group.World.CurrentDate, 100L, typeRngOffset));

        _newValue = value;
    }

    public CellCulturalKnowledge(CellGroup group, string id, string name, int typeRngOffset, int value, int asymptote) : base(id, name, value)
    {
        Group = group;
        RngOffset = unchecked((int)group.GenerateUniqueIdentifier(group.World.CurrentDate, 100L, typeRngOffset));
        Asymptote = asymptote;

        _newValue = value;
    }

    public void SetInitialValue(int value)
    {
        Value = value;
        _newValue = value;
    }

    public static CellCulturalKnowledge CreateCellInstance(string id, CellGroup group, int initialValue = 0)
    {
        switch (id)
        {
            case ShipbuildingKnowledge.ShipbuildingKnowledgeId:
                return new ShipbuildingKnowledge(group, initialValue);

            case AgricultureKnowledge.AgricultureKnowledgeId:
                return new AgricultureKnowledge(group, initialValue);

            case SocialOrganizationKnowledge.SocialOrganizationKnowledgeId:
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

        if (d > Group.GetNextLocalRandomFloat(RngOffsets.KNOWLEDGE_MERGE + RngOffset))
            mergedValue++;

#if DEBUG
        if ((Id == SocialOrganizationKnowledge.SocialOrganizationKnowledgeId) && (mergedValue < SocialOrganizationKnowledge.MinValueForHoldingTribalism))
        {
            if (Group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning("group with low social organization has faction cores - Id: " + Group.Id);
            }
        }
#endif
#if DEBUG
        if ((Group.Id == 55200582289076) &&
            (Id == SocialOrganizationKnowledge.SocialOrganizationKnowledgeId) &&
            (Value >= SocialOrganizationKnowledge.MinValueForHoldingTribalism) &&
            (mergedValue < SocialOrganizationKnowledge.MinValueForHoldingTribalism))
        {
            bool debug = true;
        }
#endif

        _newValue = mergedValue;
    }

    public virtual void Synchronize()
    {

    }

    public virtual void FinalizeLoad()
    {

    }

    public void UpdateProgressLevel()
    {
        ProgressLevel = 0;

        if (Asymptote > 0)
            ProgressLevel = MathUtility.RoundToSixDecimals(Mathf.Clamp01(Value / (float)Asymptote));
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

        int rngOffset = RngOffsets.KNOWLEDGE_UPDATE_VALUE_INTERNAL + RngOffset;

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
            Debug.LogError("UpdateValueInternal: new value " + newValue + " above Asymptote " + Asymptote);
        }
#endif
#if DEBUG
        if (newValue > 1000000)
        {
            Debug.LogError("UpdateValueInternal: new value " + newValue + " above 1000000000");
        }
#endif
#if DEBUG
        if ((Group.Id == 55200582289076) &&
            (Id == SocialOrganizationKnowledge.SocialOrganizationKnowledgeId) &&
            (Value >= SocialOrganizationKnowledge.MinValueForHoldingTribalism) &&
            (newValue < SocialOrganizationKnowledge.MinValueForHoldingTribalism) &&
            (Asymptote > SocialOrganizationKnowledge.BaseAsymptote))
        {
            bool debug = true;
        }
#endif

        _newValue = newValue;
    }

    public abstract void PolityCulturalProminence(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan);

    protected void PolityCulturalProminenceInternal(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan, float timeEffectFactor)
    {
        int rngOffset = RngOffsets.KNOWLEDGE_POLITY_PROMINENCE + RngOffset + unchecked((int)polityProminence.PolityId);

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
        if ((Group.Id == 55200582289076) &&
            (Id == SocialOrganizationKnowledge.SocialOrganizationKnowledgeId) &&
            (Value >= SocialOrganizationKnowledge.MinValueForHoldingTribalism) &&
            (_newValue < SocialOrganizationKnowledge.MinValueForHoldingTribalism))
        {
            bool debug = true;
        }
#endif

        _newValue = _newValue + valueChange;
    }

    public void PostUpdate()
    {
#if DEBUG
        if ((Group.Id == 55200582289076) && 
            (Id == SocialOrganizationKnowledge.SocialOrganizationKnowledgeId) && 
            (Value >= SocialOrganizationKnowledge.MinValueForHoldingTribalism) &&
            (_newValue < SocialOrganizationKnowledge.MinValueForHoldingTribalism))
        {
            bool debug = true;
        }
#endif

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
}

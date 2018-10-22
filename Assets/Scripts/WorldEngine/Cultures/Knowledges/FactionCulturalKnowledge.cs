using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionCulturalKnowledge : CulturalKnowledge
{
    [XmlIgnore]
    public Faction Faction;

    [XmlIgnore]
    public PolityCulturalKnowledge PolityCulturalKnowledge;

    [XmlIgnore]
    public CellCulturalKnowledge CoreCulturalKnowledge;

    public FactionCulturalKnowledge()
    {
    }

    public FactionCulturalKnowledge(Faction faction, CellCulturalKnowledge coreKnowledge, PolityCulture polityCulture) : base(coreKnowledge)
    {
        Faction = faction;

        CoreCulturalKnowledge = coreKnowledge;

        SetPolityCulturalKnowledge(polityCulture);
    }

    public void SetPolityCulturalKnowledge(PolityCulture culture)
    {
        PolityCulturalKnowledge = culture.GetKnowledge(Id) as PolityCulturalKnowledge;

        if (PolityCulturalKnowledge == null)
        {
            PolityCulturalKnowledge = new PolityCulturalKnowledge(Id, Name, 0);

            culture.AddKnowledge(PolityCulturalKnowledge);
        }
    }

    public void UpdatePolityKnowledge(float influence)
    {
        if (!IsPresent) return;
        
        Profiler.BeginSample("PolityCulturalKnowledge.Set()");

        PolityCulturalKnowledge.Set();

        Profiler.EndSample();

        Profiler.BeginSample("PolityCulturalKnowledge.AccValue");

        PolityCulturalKnowledge.AccValue += Value * influence;

        Profiler.EndSample();
    }

    public void UpdateFromCoreKnowledge(float timeFactor)
    {
        Set();

        int targetValue = 0;

        if ((CoreCulturalKnowledge != null) && CoreCulturalKnowledge.IsPresent)
        {
            targetValue = CoreCulturalKnowledge.Value;
        }

        float decimals;

        Value = MathUtility.LerpToIntAndGetDecimals(Value, targetValue, timeFactor, out decimals);

        if (decimals > Faction.GetNextLocalRandomFloat(RngOffsets.KNOWLEDGE_FACTION_CORE_UPDATE + unchecked((int)Faction.Id)))
        {
            Value++;
        }

        if (Value <= 0)
        {
            Reset();
        }
    }
}

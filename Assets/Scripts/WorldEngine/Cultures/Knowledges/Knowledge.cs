using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

// TODO: This class should replace CellCulturalKnowledge when knowledge modding is in
public class Knowledge : CellCulturalKnowledge
{
    public static Dictionary<string, Knowledge> Knowledges;

    public List<ICellGroupEventGenerator> OnUpdateEventGenerators;

    public static void ResetKnowledges()
    {
        Knowledges = new Dictionary<string, Knowledge>();
    }

    public static void InitializeKnowledges()
    { 
        // TODO: this function should use modded knowledges instead of hardcoded ones
        Knowledges.Add(ShipbuildingKnowledge.KnowledgeId, new Knowledge());
        Knowledges.Add(AgricultureKnowledge.KnowledgeId, new Knowledge());
        Knowledges.Add(SocialOrganizationKnowledge.KnowledgeId, new Knowledge());
    }

    public static Knowledge GetKnowledge(string id)
    {
        Knowledge k;

        if (!Knowledges.TryGetValue(id, out k))
        {
            return null;
        }

        return k;
    }

    public override float CalculateExpectedProgressLevel()
    {
        throw new System.NotImplementedException();
    }

    public override float CalculateTransferFactor()
    {
        throw new System.NotImplementedException();
    }

    public override void PolityCulturalProminence(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan)
    {
        throw new System.NotImplementedException();
    }

    public override bool WillBeLost()
    {
        throw new System.NotImplementedException();
    }

    protected override int CalculateLimitInternal(CulturalDiscovery discovery)
    {
        throw new System.NotImplementedException();
    }

    protected override int GetBaseLimit()
    {
        throw new System.NotImplementedException();
    }

    protected override void UpdateInternal(long timeSpan)
    {
        throw new System.NotImplementedException();
    }
}

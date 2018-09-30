using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribalismDiscovery : CellCulturalDiscovery
{
    public const string TribalismDiscoveryId = "TribalismDiscovery";
    public const string TribalismDiscoveryName = "Tribalism";

    public TribalismDiscovery() : base(TribalismDiscoveryId, TribalismDiscoveryName)
    {

    }

    public override bool CanBeHeld(CellGroup group)
    {
        CulturalKnowledge knowledge = group.Culture.GetKnowledge(SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

        if (knowledge == null)
        {
            return false;
        }

        if (knowledge.Value < SocialOrganizationKnowledge.MinValueForHoldingTribalism)
        {
#if DEBUG
            if (group.GetFactionCores().Count > 0)
            {

                Debug.LogWarning("Group that will lose tribalism has faction cores - Id: " + group.Id);
            }
#endif

            return false;
        }

        return true;
    }
}

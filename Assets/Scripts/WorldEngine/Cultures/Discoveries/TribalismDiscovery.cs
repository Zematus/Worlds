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
        int value = 0;

        if (!group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.SocialOrganizationKnowledgeId, out value))
        {
#if DEBUG
            if (group.Id == 55200582289076)
            {
                bool debug = true;
            }
#endif
            return false;
        }

        if (value < SocialOrganizationKnowledge.MinValueForHoldingTribalism)
        {
#if DEBUG
            if (group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning("Group that will lose tribalism has faction cores - Id: " + group.Id);
            }
#endif
#if DEBUG
            if (group.Id == 55200582289076)
            {
                bool debug = true;
            }
#endif
            return false;
        }

        return true;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribalismDiscovery : CellCulturalDiscovery
{
    public const string DiscoveryId = "TribalismDiscovery";
    public const string DiscoveryName = "Tribalism";

    public TribalismDiscovery() : base(DiscoveryId, DiscoveryName)
    {

    }

    public override bool CanBeHeld(CellGroup group)
    {
        int value = 0;

        if (!group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out value))
        {
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
            return false;
        }

        return true;
    }
}

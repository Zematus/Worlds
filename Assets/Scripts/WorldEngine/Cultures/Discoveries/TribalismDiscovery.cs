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

        bool canBeHeld = true;

        if (!group.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out value))
        {
            canBeHeld = false;
        }

        if (value < SocialOrganizationKnowledge.MinValueForHoldingTribalism)
        {
            canBeHeld = false;
        }

#if DEBUG
        if (!canBeHeld)
        {
            if (group.GetFactionCores().Count > 0)
            {
                Debug.LogWarning("Group that will lose tribalism has faction cores - Id: " + group.Id + ", value:" + value + ", date:" + group.World.CurrentDate);
            }

            if (group.WillBecomeFactionCore)
            {
                Debug.LogWarning("Group that will lose tribalism will become a faction core - Id: " + group.Id + ", value:" + value + ", date:" + group.World.CurrentDate);
            }
        }
#endif

        return canBeHeld;
    }
}

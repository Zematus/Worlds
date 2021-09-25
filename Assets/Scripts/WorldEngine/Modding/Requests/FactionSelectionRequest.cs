using System.Collections.Generic;
using UnityEngine;

public class FactionSelectionRequest : EntitySelectionRequest<Faction>
{
    public ModText Text { get; private set; }

    public FactionSelectionRequest(
        ICollection<Faction> collection) :
        base(collection)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }
    }
}

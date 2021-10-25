using System.Collections.Generic;
using UnityEngine;

public class ContactSelectionRequest : EntitySelectionRequest<PolityContact>, IMapEntitySelectionRequest
{
    private readonly HashSet<PolityContact> _involvedContacts = null;

    public ModText Text { get; private set; }

    public ContactSelectionRequest(
        ICollection<PolityContact> collection,
        ModText text) :
        base(collection)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        Text = text;

        _involvedContacts = new HashSet<PolityContact>(collection);
    }

    public RectInt GetEncompassingRectangle()
    {
        RectInt rect = new RectInt();

        int worldWidth = Manager.CurrentWorld.Width;

        bool first = true;
        foreach (PolityContact contact in _involvedContacts)
        {
            RectInt rRect = contact.NeighborPolity.Territory.GetBoundingRectangle();

            if (first)
            {
                rect.SetMinMax(rRect.min, rRect.max);

                first = false;
                continue;
            }

            rect.Extend(rRect, worldWidth);
        }

        return rect;
    }
}

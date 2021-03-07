using System.Collections.Generic;

public class RegionSelectionRequest : EntitySelectionRequest<Region>
{
    private HashSet<Region> _involvedRegions = null;

    public ModText Text { get; private set; }

    public RegionSelectionRequest(
        ICollection<Region> collection,
        ModText text) :
        base(collection)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        Text = text;

        Polity guidedPolity = guidedFaction.Polity;

        _involvedRegions = new HashSet<Region>(guidedPolity.CoreRegions);
        _involvedRegions.IntersectWith(collection);

        // Set involved regions as filtered so that the UI can quickly filter them

        foreach (Region region in guidedPolity.CoreRegions)
        {
            region.AssignedFilterType = Region.FilterType.Core;
        }

        foreach (Region region in collection)
        {
            region.AssignedFilterType = Region.FilterType.Selectable;
        }
    }

    public override void Close()
    {
        foreach (Region region in _involvedRegions)
        {
            region.AssignedFilterType = Region.FilterType.None;
        }

        base.Close();
    }
}

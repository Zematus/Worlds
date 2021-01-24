using System.Collections.Generic;

public class RegionSelectionRequest : EntitySelectionRequest<Region>
{
    private HashSet<Region> _involvedRegions = null;

    public RegionSelectionRequest(ICollection<Region> collection) : base(collection)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        Polity guidedPolity = guidedFaction.Polity;

        _involvedRegions = new HashSet<Region>(guidedPolity.CoreRegions);
        _involvedRegions.IntersectWith(collection);

        // Set involved regions as filtered so that the UI can quickly filter them

        foreach (Region region in _involvedRegions)
        {
            region.IsUiFilteredIn = true;
        }
    }

    public override void Close()
    {
        foreach (Region region in _involvedRegions)
        {
            region.IsUiFilteredIn = false;
        }

        base.Close();
    }
}

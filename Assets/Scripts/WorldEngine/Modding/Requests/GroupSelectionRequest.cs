using System.Collections.Generic;

public class GroupSelectionRequest : EntitySelectionRequest<CellGroup>, IMapEntitySelectionRequest
{
    private readonly HashSet<CellGroup> _involvedGroups = null;

    public ModText Text { get; private set; }

    public GroupSelectionRequest(
        ICollection<CellGroup> collection,
        ModText text) :
        base(collection)
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("Can't create request without an active guided faction");
        }

        Text = text;

        _involvedGroups = new HashSet<CellGroup>(collection);
        _involvedGroups.Add(guidedFaction.CoreGroup);

        // Set involved groups as filtered so that the UI can quickly filter them

        guidedFaction.CoreGroup.AssignedFilterType = CellGroup.FilterType.Core;

        foreach (var group in collection)
        {
            group.AssignedFilterType = CellGroup.FilterType.Selectable;
        }
    }

    public override void Close()
    {
        foreach (var group in _involvedGroups)
        {
            group.AssignedFilterType = CellGroup.FilterType.None;
        }

        base.Close();
    }
}
